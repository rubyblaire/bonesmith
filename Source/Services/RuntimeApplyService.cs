using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace BoneSmith.Services;

public sealed class RuntimeApplyService : IDisposable
{
    private readonly Configuration configuration;
    private readonly IDalamudPluginInterface pluginInterface;
    private readonly IPluginLog log;
    private readonly RuntimePayloadService payloadService;
    private readonly LocalPlayerApplyService localPlayerApplyService;
    private readonly RenderHookApplyService renderHookApplyService;

    private bool running;
    private DateTimeOffset? startedAt;

    public bool IsRunning => running;
    public long TickCount => renderHookApplyService.RenderTicks;
    public DateTimeOffset? StartedAt => startedAt;
    public string Status { get; private set; } = "Runtime stopped.";

    public RuntimeApplyService(
        Configuration configuration,
        IDalamudPluginInterface pluginInterface,
        IPluginLog log,
        RuntimePayloadService payloadService,
        LocalPlayerApplyService localPlayerApplyService,
        RenderHookApplyService renderHookApplyService)
    {
        this.configuration = configuration;
        this.pluginInterface = pluginInterface;
        this.log = log;
        this.payloadService = payloadService;
        this.localPlayerApplyService = localPlayerApplyService;
        this.renderHookApplyService = renderHookApplyService;
    }

    public void Start()
    {
        if (running)
            return;

        EnforceProfileOnlyMode();

        var payload = configuration.AutoBuildPayloadOnApply
            ? payloadService.BuildPayload()
            : payloadService.LastPayload ?? payloadService.BuildPayload();

        if (payload is null)
        {
            Status = "Cannot start runtime: no active profile payload is staged.";
            configuration.LastRuntimeStatus = Status;
            pluginInterface.SavePluginConfig(configuration);
            return;
        }

        try
        {
            localPlayerApplyService.Start();
            renderHookApplyService.Enable();

            running = true;
            startedAt = DateTimeOffset.Now;
            Status = "Runtime active. Applying local-player profile transforms during render hook.";

            configuration.LastApplyStartedAt = startedAt;
            configuration.LastAppliedProfileName = configuration.ActiveProfileName;
            configuration.LastAppliedTemplateName = configuration.ActiveTemplateName;
            configuration.LastAppliedPayloadMode = configuration.PayloadBuildMode.ToString();
            configuration.CompatibilityRuntimeEnabled = true;
            configuration.LastRuntimeStatus = Status;
            pluginInterface.SavePluginConfig(configuration);

            log.Information("BoneSmith render-hook runtime started.");
        }
        catch (Exception ex)
        {
            renderHookApplyService.Disable();
            localPlayerApplyService.StopAndResetLocalScale();

            running = false;
            Status = $"Runtime failed to start: {ex.GetType().Name}: {ex.Message}";

            configuration.LastApplyStoppedAt = DateTimeOffset.Now;
            configuration.LastPanicResetReason = "runtime failed to start";
            configuration.CompatibilityRuntimeEnabled = false;
            configuration.LastRuntimeStatus = Status;
            pluginInterface.SavePluginConfig(configuration);

            log.Error(ex, "BoneSmith runtime failed to start.");
        }
    }

    public void Stop()
    {
        if (!running)
            return;

        renderHookApplyService.Disable();

        if (configuration.ResetLocalScaleOnStop)
            localPlayerApplyService.StopAndResetLocalScale();
        else
            localPlayerApplyService.Stop();

        running = false;
        Status = "Runtime stopped.";

        configuration.LastApplyStoppedAt = DateTimeOffset.Now;
        configuration.CompatibilityRuntimeEnabled = false;
        configuration.LastRuntimeStatus = Status;
        pluginInterface.SavePluginConfig(configuration);

        log.Information("BoneSmith render-hook runtime stopped.");
    }

    public void StopForSafety(string reason)
    {
        if (!running)
            return;

        renderHookApplyService.Disable();

        if (configuration.ResetLocalScaleOnStop)
            localPlayerApplyService.StopAndResetLocalScale();
        else
            localPlayerApplyService.Stop();

        running = false;
        Status = $"Runtime stopped for safety: {reason}";

        configuration.LastApplyStoppedAt = DateTimeOffset.Now;
        configuration.LastPanicResetReason = reason;
        configuration.CompatibilityRuntimeEnabled = false;
        configuration.LastRuntimeStatus = Status;
        pluginInterface.SavePluginConfig(configuration);

        log.Warning("BoneSmith runtime stopped for safety: {Reason}", reason);
    }

    public void StopAndResetNow()
    {
        PanicReset("manual stop + reset");
    }

    public void PanicReset(string reason)
    {
        renderHookApplyService.Disable();
        localPlayerApplyService.StopAndResetLocalScale();

        running = false;
        Status = $"PANIC RESET complete: {reason}";

        configuration.LastApplyStoppedAt = DateTimeOffset.Now;
        configuration.LastPanicResetReason = reason;

        if (configuration.PanicResetDisablesAutoApply)
            configuration.AutoApplyLastSelectionOnLogin = false;

        configuration.CompatibilityRuntimeEnabled = false;
        configuration.LastRuntimeStatus = Status;
        pluginInterface.SavePluginConfig(configuration);

        log.Warning("BoneSmith panic reset completed: {Reason}", reason);
    }

    private void EnforceProfileOnlyMode()
    {
        if (!configuration.AllowStandaloneTemplateApply)
            configuration.PayloadBuildMode = BoneSmith.Models.PayloadBuildMode.ProfileOnly;
    }


    public void ReleaseCurrentSelection()
    {
        renderHookApplyService.Disable();
        localPlayerApplyService.StopAndResetLocalScale();

        running = false;
        Status = "Released active profile/template selection.";

        configuration.ActiveProfileId = null;
        configuration.ActiveProfileName = null;
        configuration.ActiveProfilePath = null;

        configuration.ActiveTemplateId = null;
        configuration.ActiveTemplateName = null;
        configuration.ActiveTemplatePath = null;

        configuration.PayloadBuildMode = BoneSmith.Models.PayloadBuildMode.ProfileOnly;

        configuration.LastRuntimePayloadPath = null;
        configuration.LastRuntimePayloadBuiltAt = null;
        configuration.LastRuntimeStatus = Status;
        configuration.LastApplyStoppedAt = DateTimeOffset.Now;
        configuration.LastPanicResetReason = "released active selection";
        configuration.CompatibilityRuntimeEnabled = false;

        pluginInterface.SavePluginConfig(configuration);

        payloadService.ClearPayload();

        log.Warning("BoneSmith released active profile/template selection.");
    }

    public void ApplyCurrentSelection()
    {
        EnforceProfileOnlyMode();
        Start();
    }

    public void Dispose()
    {
        Stop();
        renderHookApplyService.Dispose();
    }

    public string GetRenderHookStatus()
    {
        return renderHookApplyService.Status;
    }

    public bool IsRenderHookEnabled()
    {
        return renderHookApplyService.IsHookEnabled;
    }

    public bool RenderHookFailed()
    {
        return renderHookApplyService.HookFailed;
    }

    public int RenderHookExceptions()
    {
        return renderHookApplyService.ConsecutiveExceptions;
    }

    public DateTimeOffset? LastRenderHookExceptionAt()
    {
        return renderHookApplyService.LastExceptionAt;
    }
}
