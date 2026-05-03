using System.Runtime.InteropServices;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;

namespace BoneSmith.Services;

public sealed class RenderHookApplyService : IDisposable
{
    // Same render hook signature/address used by Customize+.
    private const string RenderHookAddress = "E8 ?? ?? ?? ?? 48 81 C3 ?? ?? ?? ?? BF ?? ?? ?? ?? 33 ED";

    private readonly ISigScanner sigScanner;
    private readonly IGameInteropProvider interopProvider;
    private readonly IPluginLog log;
    private readonly LocalPlayerApplyService localPlayerApplyService;

    private Hook<RenderDelegate>? renderHook;
    private bool hookFailed;

    private delegate nint RenderDelegate(nint a1, nint a2, nint a3, int a4);

    public bool IsHookEnabled => renderHook?.IsEnabled ?? false;
    public bool HookFailed => hookFailed;
    public string Status { get; private set; } = "Render hook not enabled.";
    public long RenderTicks { get; private set; }
    public int ConsecutiveExceptions { get; private set; }
    public DateTimeOffset? LastExceptionAt { get; private set; }

    public RenderHookApplyService(
        ISigScanner sigScanner,
        IGameInteropProvider interopProvider,
        IPluginLog log,
        LocalPlayerApplyService localPlayerApplyService)
    {
        this.sigScanner = sigScanner;
        this.interopProvider = interopProvider;
        this.log = log;
        this.localPlayerApplyService = localPlayerApplyService;
    }

    public void Enable()
    {
        if (renderHook?.IsEnabled == true)
            return;

        hookFailed = false;

        try
        {
            if (renderHook is null)
            {
                var renderAddress = sigScanner.ScanText(RenderHookAddress);
                renderHook = interopProvider.HookFromAddress<RenderDelegate>(renderAddress, OnRender);
                Status = $"Render hook created at 0x{renderAddress:X}.";
                log.Information("BoneSmith render hook created at {Address:X}", renderAddress);
            }

            renderHook.Enable();
            ConsecutiveExceptions = 0;
            LastExceptionAt = null;
            Status = "Render hook enabled. Applying during render.";
            log.Information("BoneSmith render hook enabled.");
        }
        catch (Exception ex)
        {
            hookFailed = true;
            Status = $"Render hook failed: {ex.GetType().Name}: {ex.Message}";
            log.Error(ex, "BoneSmith failed to enable render hook.");
            throw;
        }
    }

    public void Disable()
    {
        try
        {
            renderHook?.Disable();
            Status = "Render hook disabled.";
            log.Information("BoneSmith render hook disabled.");
        }
        catch (Exception ex)
        {
            Status = $"Render hook disable failed: {ex.GetType().Name}: {ex.Message}";
            log.Error(ex, "BoneSmith failed to disable render hook.");
        }
    }

    public void Dispose()
    {
        try
        {
            renderHook?.Disable();
            renderHook?.Dispose();
            renderHook = null;
        }
        catch (Exception ex)
        {
            log.Error(ex, "BoneSmith failed to dispose render hook.");
        }
    }

    private nint OnRender(nint a1, nint a2, nint a3, int a4)
    {
        if (renderHook is null)
            return nint.Zero;

        try
        {
            RenderTicks++;

            // Match the important Customize+ timing idea:
            // apply transforms inside the render hook before calling original render function.
            localPlayerApplyService.Tick();
        }
        catch (Exception ex)
        {
            hookFailed = true;
            ConsecutiveExceptions++;
            LastExceptionAt = DateTimeOffset.Now;
            Status = $"Render hook apply exception, disabling hook: {ex.GetType().Name}: {ex.Message}";
            log.Error(ex, "BoneSmith render hook apply failed.");

            try
            {
                renderHook.Disable();
            }
            catch
            {
                // ignored
            }
        }

        return renderHook.Original(a1, a2, a3, a4);
    }
}
