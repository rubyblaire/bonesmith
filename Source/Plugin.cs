using BoneSmith.Services;
using BoneSmith.UI;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace BoneSmith;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "BoneSmith";

    private const string CommandName = "/bonesmith";

    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static ISigScanner SigScanner { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;

    private readonly WindowSystem windowSystem = new("BoneSmith");
    private readonly MainWindow mainWindow;

    private bool attemptedAutoApply;
    private DateTimeOffset loadedAt = DateTimeOffset.Now;

    internal Configuration Configuration { get; }
    internal CustomizePlusDataService CustomizePlusDataService { get; }
    internal CustomizePlusRuntimeParser RuntimeParser { get; }
    internal RuntimePayloadService RuntimePayloadService { get; }
    internal LocalPlayerApplyService LocalPlayerApplyService { get; }
    internal RenderHookApplyService RenderHookApplyService { get; }
    internal RuntimeApplyService RuntimeApplyService { get; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        Configuration.WriteBackToCustomizePlus = false;

        CustomizePlusDataService = new CustomizePlusDataService(Configuration, PluginInterface, Log);
        RuntimeParser = new CustomizePlusRuntimeParser(CustomizePlusDataService);
        RuntimePayloadService = new RuntimePayloadService(Configuration, PluginInterface, Log, RuntimeParser);
        LocalPlayerApplyService = new LocalPlayerApplyService(Configuration, ObjectTable, Log, RuntimePayloadService);
        RenderHookApplyService = new RenderHookApplyService(SigScanner, GameInteropProvider, Log, LocalPlayerApplyService);
        RuntimeApplyService = new RuntimeApplyService(Configuration, PluginInterface, Log, RuntimePayloadService, LocalPlayerApplyService, RenderHookApplyService);

        mainWindow = new MainWindow(this);
        windowSystem.AddWindow(mainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open BoneSmith."
        });

        PluginInterface.UiBuilder.Draw += DrawUi;
        PluginInterface.UiBuilder.OpenMainUi += OpenMainUi;
        ClientState.TerritoryChanged += OnTerritoryChanged;

        if (Configuration.AutoDetectCustomizePlus)
        {
            CustomizePlusDataService.RefreshDetection();
            SaveConfig();
        }

        Log.Information("BoneSmith v0.14.1 loaded.");
    }

    public void Dispose()
    {
        ClientState.TerritoryChanged -= OnTerritoryChanged;
        RuntimeApplyService.PanicReset("plugin dispose");
        RuntimeApplyService.Dispose();

        PluginInterface.UiBuilder.Draw -= DrawUi;
        PluginInterface.UiBuilder.OpenMainUi -= OpenMainUi;

        CommandManager.RemoveHandler(CommandName);

        windowSystem.RemoveAllWindows();
        mainWindow.Dispose();

        Log.Information("BoneSmith disposed.");
    }

    internal void SaveConfig()
    {
        PluginInterface.SavePluginConfig(Configuration);
    }

    private void OnCommand(string command, string args)
    {
        mainWindow.IsOpen = true;
    }

    private void OpenMainUi()
    {
        mainWindow.IsOpen = true;
    }

    private void OnTerritoryChanged(uint territoryType)
    {
        attemptedAutoApply = false;

        if (Configuration.StopRuntimeOnTerritoryChange && RuntimeApplyService.IsRunning)
            RuntimeApplyService.StopForSafety($"territory changed to {territoryType}");
    }

    private void MaybeAutoApplyLastSelection()
    {
        if (attemptedAutoApply)
            return;

        if (!Configuration.AutoApplyLastSelectionOnLogin)
            return;

        if (Configuration.RequireManualConfirmForAutoApply)
            return;

        // Give the object table / character skeleton a few seconds to settle after plugin load or zone load.
        if (DateTimeOffset.Now - loadedAt < TimeSpan.FromSeconds(8))
            return;

        if (RuntimeApplyService.IsRunning)
            return;

        attemptedAutoApply = true;

        try
        {
            if (!string.IsNullOrWhiteSpace(Configuration.ActiveProfilePath) ||
                !string.IsNullOrWhiteSpace(Configuration.ActiveTemplatePath))
            {
                RuntimeApplyService.ApplyCurrentSelection();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "BoneSmith auto-apply failed.");
            RuntimeApplyService.PanicReset("auto-apply failed");
        }
    }

    private void DrawUi()
    {
        MaybeAutoApplyLastSelection();
        windowSystem.Draw();
    }
}
