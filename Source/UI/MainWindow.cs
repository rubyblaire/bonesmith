using System.Diagnostics;
using System.Numerics;
using BoneSmith.Models;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace BoneSmith.UI;

public sealed class MainWindow : Window, IDisposable
{
    private const string PluginVersion = "0.15.0";

    private readonly Plugin plugin;
    private string? lastBackupPath;

    private static readonly Vector4 Text = new(0.92f, 0.93f, 0.96f, 1.00f);
    private static readonly Vector4 Muted = new(0.62f, 0.62f, 0.74f, 1.00f);
    private static readonly Vector4 WindowBg = new(0.015f, 0.015f, 0.020f, 1.00f);
    private static readonly Vector4 Panel = new(0.050f, 0.040f, 0.080f, 1.00f);
    private static readonly Vector4 PanelHover = new(0.080f, 0.060f, 0.130f, 1.00f);
    private static readonly Vector4 Violet = new(0.49f, 0.23f, 0.92f, 1.00f);
    private static readonly Vector4 VioletHover = new(0.66f, 0.30f, 1.00f, 1.00f);
    private static readonly Vector4 VioletActive = new(0.33f, 0.12f, 0.72f, 1.00f);
    private static readonly Vector4 Border = new(0.28f, 0.18f, 0.50f, 1.00f);
    private static readonly Vector4 Obsidian = new(0.015f, 0.013f, 0.018f, 1.00f);
    private static readonly Vector4 ObsidianActive = new(0.030f, 0.026f, 0.038f, 1.00f);
    private static readonly Vector4 ObsidianCollapsed = new(0.010f, 0.009f, 0.014f, 1.00f);
    private static readonly Vector4 Ember = new(0.98f, 0.58f, 0.18f, 1.00f);
    private static readonly Vector4 EmberHover = new(1.00f, 0.70f, 0.28f, 1.00f);
    private static readonly Vector4 EmberActive = new(0.78f, 0.32f, 0.08f, 1.00f);
    private static readonly Vector4 EmberToggleBg = new(0.16f, 0.065f, 0.015f, 1.00f);
    private static readonly Vector4 EmberToggleHover = new(0.26f, 0.11f, 0.025f, 1.00f);
    private static readonly Vector4 EmberToggleActive = new(0.36f, 0.16f, 0.040f, 1.00f);
    private static readonly Vector4 Good = new(0.38f, 0.86f, 0.62f, 1.00f);
    private static readonly Vector4 Warn = new(1.00f, 0.78f, 0.30f, 1.00f);
    private static readonly Vector4 Bad = new(1.00f, 0.34f, 0.34f, 1.00f);

    public MainWindow(Plugin plugin)
        : base("BoneSmith###BoneSmithMainWindow")
    {
        this.plugin = plugin;

        Size = new Vector2(820, 640);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public void Dispose()
    {
    }

    public override void PreDraw()
    {
        PushTheme();
    }

    public override void Draw()
    {
        DrawHeader();

        if (!plugin.Configuration.ConfirmedExperimentalApplyWarning)
            DrawWarningPanel();

        if (ImGui.BeginTabBar("BoneSmithTabs"))
        {
            if (ImGui.BeginTabItem("Home"))
            {
                DrawHomeTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Profiles"))
            {
                DrawProfilesTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Recovery"))
            {
                DrawRecoveryTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Settings"))
            {
                DrawSettingsTab();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    public override void PostDraw()
    {
        PopTheme();
    }

    private static void PushTheme()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 10f);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 8f);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 6f);
        ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding, 6f);
        ImGui.PushStyleVar(ImGuiStyleVar.TabRounding, 6f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(16f, 14f));
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(10f, 6f));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(10f, 8f));

        ImGui.PushStyleColor(ImGuiCol.Text, Text);
        ImGui.PushStyleColor(ImGuiCol.TextDisabled, Muted);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, WindowBg);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, Panel);
        ImGui.PushStyleColor(ImGuiCol.PopupBg, Panel);
        ImGui.PushStyleColor(ImGuiCol.Border, Border);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, Panel);
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, PanelHover);
        ImGui.PushStyleColor(ImGuiCol.FrameBgActive, VioletActive);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, Obsidian);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, ObsidianActive);
        ImGui.PushStyleColor(ImGuiCol.TitleBgCollapsed, ObsidianCollapsed);
        ImGui.PushStyleColor(ImGuiCol.Button, Violet);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, VioletHover);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, VioletActive);
        ImGui.PushStyleColor(ImGuiCol.CheckMark, VioletHover);
        ImGui.PushStyleColor(ImGuiCol.SliderGrab, Violet);
        ImGui.PushStyleColor(ImGuiCol.SliderGrabActive, VioletHover);
        ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.22f, 0.10f, 0.03f, 1f));
        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.44f, 0.20f, 0.05f, 1f));
        ImGui.PushStyleColor(ImGuiCol.HeaderActive, EmberActive);
        ImGui.PushStyleColor(ImGuiCol.Tab, new Vector4(0.12f, 0.05f, 0.015f, 1f));
        ImGui.PushStyleColor(ImGuiCol.TabHovered, EmberHover);
        ImGui.PushStyleColor(ImGuiCol.TabActive, Ember);
        ImGui.PushStyleColor(ImGuiCol.TabUnfocused, new Vector4(0.045f, 0.025f, 0.015f, 1f));
        ImGui.PushStyleColor(ImGuiCol.TabUnfocusedActive, new Vector4(0.30f, 0.12f, 0.03f, 1f));
    }

    private static void PopTheme()
    {
        ImGui.PopStyleColor(26);
        ImGui.PopStyleVar(8);
    }

    private void DrawHeader()
    {
        ImGui.BeginChild("BoneSmithHeader", new Vector2(0, 72), true);

        ImGui.TextColored(EmberHover, "BoneSmith");
        ImGui.TextColored(Muted, "A Ruby Blaire plugin • Customize+ profile bridge");

        ImGui.Spacing();

        DrawStatusPill(plugin.RuntimeApplyService.IsRunning ? "ACTIVE" : "READY", plugin.RuntimeApplyService.IsRunning ? Good : Muted);
        ImGui.SameLine();
        DrawStatusPill("PROFILE ONLY", Ember);
        ImGui.SameLine();
        DrawStatusPill(plugin.RuntimeApplyService.IsRenderHookEnabled() ? "HOOK ENABLED" : "HOOK OFF", plugin.RuntimeApplyService.IsRenderHookEnabled() ? Good : Muted);
        ImGui.SameLine();
        ImGui.TextColored(Muted, $"Profile: {plugin.Configuration.ActiveProfileName ?? "none"}");

        ImGui.EndChild();
        ImGui.Spacing();
    }

    private static void DrawStatusPill(string text, Vector4 color)
    {
        ImGui.TextColored(color, text);
    }

    private void DrawWarningPanel()
    {
        ImGui.BeginChild("BoneSmithWarning", new Vector2(0, 86), true);
        ImGui.TextColored(Warn, "Experimental apply warning");
        ImGui.TextWrapped("BoneSmith applies Customize+ profile data to the local player through a private render-hook runtime. Use Reset if anything sticks or feels wrong.");

        if (ImGui.Button("I understand this is experimental"))
        {
            plugin.Configuration.ConfirmedExperimentalApplyWarning = true;
            plugin.SaveConfig();
        }

        ImGui.EndChild();
        ImGui.Spacing();
    }

    private void DrawHomeTab()
    {
        ImGui.BeginChild("HomeMain", new Vector2(0, 0), false);

        DrawHomeStatusSummary();

        ImGui.Spacing();

        DrawSectionTitle("Actions");

        if (ImGui.Button("Apply Current Selection", new Vector2(220, 34)))
            ApplyCurrentSelection();
        DrawTooltip("Applies this profile through BoneSmith without editing Customize+.");

        ImGui.SameLine();

        if (ImGui.Button("Release Selection", new Vector2(180, 34)))
            plugin.RuntimeApplyService.ReleaseCurrentSelection();
        DrawTooltip("Releases BoneSmith's current local-player apply state while keeping your selected profile available.");

        ImGui.SameLine();

        if (DrawEmberButton("Reset", new Vector2(120, 34)))
            plugin.RuntimeApplyService.PanicReset("manual reset from home");
        DrawTooltip("Stops BoneSmith tracking and releases active selections.");

        ImGui.Spacing();

        DrawSectionTitle("Last Action");
        DrawKeyValue("Last applied profile", plugin.Configuration.LastAppliedProfileName ?? "(none)");
        DrawKeyValue("Last started", plugin.Configuration.LastApplyStartedAt?.ToString("g") ?? "(none)");
        DrawKeyValue("Last stopped", plugin.Configuration.LastApplyStoppedAt?.ToString("g") ?? "(none)");
        DrawKeyValue("Last reset reason", plugin.Configuration.LastPanicResetReason ?? "(none)");

        ImGui.EndChild();
    }

    private void DrawHomeStatusSummary()
    {
        var active = GetActiveProfile();
        var runtimeState = GetRuntimeState(active);
        var templateSummary = active is null ? "n/a" : GetEnabledTemplateSummary(active);

        DrawSectionTitle("Status Summary");

        ImGui.BeginChild("HomeStatusSummary", new Vector2(0, 128), true);
        DrawKeyValue("Selected profile", active?.DisplayName ?? "Choose a profile");
        DrawKeyValue("Active templates", templateSummary);
        DrawKeyValue("Runtime state", runtimeState);
        DrawKeyValue("Render hook", plugin.RuntimeApplyService.IsRenderHookEnabled() ? "Ready" : "Off");

        if (active is not null)
            DrawPathValue("Profile path", active.FullPath);

        ImGui.EndChild();
    }

    private LocalJsonFileItem? GetActiveProfile()
    {
        var activePath = plugin.Configuration.ActiveProfilePath;
        if (string.IsNullOrWhiteSpace(activePath))
            return null;

        return plugin.CustomizePlusDataService.LastScanResult.Profiles
            .FirstOrDefault(x => string.Equals(x.FullPath, activePath, StringComparison.OrdinalIgnoreCase));
    }

    private string GetRuntimeState(LocalJsonFileItem? active)
    {
        if (plugin.RuntimeApplyService.IsRunning)
            return "Applied";

        if (active is null)
            return "No profile selected";

        return "Ready";
    }

    private string GetEnabledTemplateSummary(LocalJsonFileItem profile)
    {
        var refs = GetTemplateReferences(profile);
        if (refs.Count == 0)
            return "0 / 0";

        var enabled = refs.Count(reference => plugin.Configuration.GetTemplateEnabledForProfile(profile.Id, profile.FullPath, reference.TemplateId, reference.EnabledInProfile));
        return $"{enabled} / {refs.Count}";
    }

    private void DrawProfilesTab()
    {
        var items = plugin.CustomizePlusDataService.LastScanResult.Profiles;
        var activePath = plugin.Configuration.ActiveProfilePath;
        var active = items.FirstOrDefault(x => string.Equals(x.FullPath, activePath, StringComparison.OrdinalIgnoreCase));

        if (items.Count == 0)
        {
            DrawEmptyState("No local Customize+ profiles found.", "Open Recovery and refresh detection if you recently added profiles.");
            return;
        }

        ImGui.TextColored(Muted, $"{items.Count} local Customize+ profiles detected. Choose one profile, then apply it from Home.");
        ImGui.Spacing();

        var available = ImGui.GetContentRegionAvail();
        var leftWidth = MathF.Max(260f, available.X * 0.42f);

        ImGui.BeginChild("ProfilesList", new Vector2(leftWidth, 0), true);
        DrawSectionTitle("Profiles");

        foreach (var item in items.OrderBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase))
        {
            var isActive = string.Equals(item.FullPath, activePath, StringComparison.OrdinalIgnoreCase);
            var label = isActive ? $"* {item.DisplayName}##{item.FullPath}" : $"{item.DisplayName}##{item.FullPath}";

            if (ImGui.Selectable(label, isActive))
            {
                plugin.CustomizePlusDataService.UseProfile(item);
                plugin.Configuration.PayloadBuildMode = PayloadBuildMode.ProfileOnly;
                plugin.SaveConfig();
                active = item;
                activePath = item.FullPath;
            }
        }

        ImGui.EndChild();
        ImGui.SameLine();

        ImGui.BeginChild("ProfileDetails", new Vector2(0, 0), true);
        DrawSectionTitle("Selected Profile");

        if (active is null)
        {
            ImGui.TextColored(Muted, "No profile selected yet.");
            ImGui.TextWrapped("Pick a profile from the list. BoneSmith will use that profile and resolve any linked templates internally.");
        }
        else
        {
            DrawBadge("Active", Good);
            ImGui.SameLine();
            DrawBadge("Profile Only", Ember);
            ImGui.Spacing();

            DrawKeyValue("Name", active.DisplayName);
            DrawKeyValue("File", active.FileName);
            DrawKeyValue("Modified", active.LastWriteTime.ToString("g"));
            DrawKeyValue("Linked templates", active.LinkedTemplateIds.Count.ToString());
            DrawPathValue("Path", active.FullPath);

            ImGui.Spacing();

            if (ImGui.Button("Apply This Profile", new Vector2(180, 34)))
                ApplyCurrentSelection();
            DrawTooltip("Applies this profile through BoneSmith without editing Customize+.");

            ImGui.SameLine();

            if (DrawEmberButton("Reset", new Vector2(110, 34)))
                plugin.RuntimeApplyService.PanicReset("manual reset from profiles");
            DrawTooltip("Stops BoneSmith tracking and releases active selections.");

            ImGui.Spacing();
            DrawProfileTemplateToggles(active);
        }

        ImGui.EndChild();
    }

    private void DrawProfileTemplateToggles(LocalJsonFileItem active)
    {
        var refs = GetTemplateReferences(active);

        DrawSectionTitle("Referenced Templates");

        if (refs.Count == 0)
        {
            ImGui.TextColored(Muted, "This profile does not reference any templates BoneSmith can read yet.");
            return;
        }

        ImGui.TextWrapped("Toggle templates for BoneSmith's runtime apply. This does not rewrite or edit your Customize+ profile.");

        if (ImGui.Button("Restore Profile Defaults", new Vector2(190, 30)))
        {
            plugin.Configuration.ClearTemplateOverridesForProfile(active.Id, active.FullPath);
            plugin.SaveConfig();
        }
        DrawTooltip("Clears BoneSmith-only template toggle choices for this profile and returns to the Customize+ defaults.");

        ImGui.Spacing();

        foreach (var reference in refs)
        {
            var template = plugin.CustomizePlusDataService.LastScanResult.Templates
                .FirstOrDefault(x => string.Equals(x.Id, reference.TemplateId, StringComparison.OrdinalIgnoreCase));
            var templateName = template?.DisplayName ?? $"Missing template ({ShortId(reference.TemplateId)})";
            var enabled = plugin.Configuration.GetTemplateEnabledForProfile(active.Id, active.FullPath, reference.TemplateId, reference.EnabledInProfile);
            var wasEnabled = enabled;

            ImGui.PushStyleColor(ImGuiCol.FrameBg, EmberToggleBg);
            ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, EmberToggleHover);
            ImGui.PushStyleColor(ImGuiCol.FrameBgActive, EmberToggleActive);
            ImGui.PushStyleColor(ImGuiCol.CheckMark, Ember);
            var templateToggleChanged = ImGui.Checkbox($"{templateName}##templateToggle{active.Id}{reference.TemplateId}", ref enabled);
            ImGui.PopStyleColor(4);

            if (templateToggleChanged)
            {
                plugin.Configuration.SetTemplateEnabledForProfile(active.Id, active.FullPath, reference.TemplateId, enabled);
                plugin.SaveConfig();
            }

            DrawTooltip("Controls whether BoneSmith includes this template when applying this profile.");

            if (template is not null)
            {
                ImGui.SameLine();
                ImGui.TextColored(Muted, $"{template.TransformLikeNodeCount} edits");
            }

            if (wasEnabled != enabled)
                ImGui.TextColored(Muted, "Saved for BoneSmith runtime only.");
        }
    }

    private static List<ProfileTemplateReferenceInfo> GetTemplateReferences(LocalJsonFileItem profile)
    {
        return profile.ReferencedTemplates.Count > 0
            ? profile.ReferencedTemplates
            : profile.LinkedTemplateIds.Select(id => new ProfileTemplateReferenceInfo
            {
                TemplateId = id,
                EnabledInProfile = true
            }).ToList();
    }

    private static string ShortId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return "unknown";

        return id.Length <= 8 ? id : id[..8];
    }

    private void DrawRecoveryTab()
    {
        var scan = plugin.CustomizePlusDataService.LastScanResult;

        ImGui.BeginChild("RecoveryMain", new Vector2(0, 0), false);

        DrawSectionTitle("Recovery Tools");
        ImGui.TextColored(Muted, "Use these if a profile sticks, detection looks stale, or you want a backup before testing.");
        ImGui.Spacing();

        if (ImGui.Button("Refresh Detection", new Vector2(170, 34)))
        {
            plugin.CustomizePlusDataService.RefreshDetection();
            plugin.SaveConfig();
        }
        DrawTooltip("Scans your local Customize+ folders again for profiles and templates.");

        ImGui.SameLine();

        if (ImGui.Button("Create Full Backup", new Vector2(170, 34)))
            lastBackupPath = plugin.CustomizePlusDataService.CreateFullBackup();
        DrawTooltip("Creates a safety copy of the detected Customize+ data before deeper testing.");

        ImGui.SameLine();

        if (ImGui.Button("Release Selection", new Vector2(170, 34)))
            plugin.RuntimeApplyService.ReleaseCurrentSelection();
        DrawTooltip("Releases BoneSmith's current local-player apply state while keeping your selected profile available.");

        ImGui.SameLine();

        if (DrawEmberButton("Reset Everything", new Vector2(170, 34)))
            plugin.RuntimeApplyService.PanicReset("manual reset from recovery");
        DrawTooltip("Stops BoneSmith tracking and releases active selections.");

        ImGui.Spacing();
        DrawSectionTitle("Customize+ Detection");

        DrawDetectionRow("Config", scan.FoundConfig, scan.ConfigPath ?? "(none)");
        DrawDetectionRow("Data folder", scan.FoundDataFolder, scan.DataFolderPath ?? "(none)");
        DrawDetectionRow("Profiles folder", scan.FoundProfilesFolder, scan.ProfilesFolderPath ?? "(none)");
        DrawDetectionRow("Templates folder", scan.FoundTemplatesFolder, scan.TemplatesFolderPath ?? "(none)");
        DrawKeyValue("Profiles", scan.Profiles.Count.ToString());
        DrawKeyValue("Templates loaded internally", scan.Templates.Count.ToString());

        if (!string.IsNullOrWhiteSpace(lastBackupPath))
            DrawPathValue("Backup created", lastBackupPath);

        if (!string.IsNullOrWhiteSpace(scan.Error))
            DrawKeyValue("Error", scan.Error);

        ImGui.Spacing();

        if (ImGui.CollapsingHeader("Advanced diagnostics"))
            DrawAdvancedDiagnostics();

        ImGui.EndChild();
    }

    private void DrawSettingsTab()
    {
        ImGui.BeginChild("SettingsMain", new Vector2(0, 0), false);

        DrawSectionTitle("About BoneSmith");
        ImGui.TextWrapped("BoneSmith is a Customize+ profile bridge for local-player testing. Normal use applies a selected Customize+ profile and resolves referenced templates internally without writing back to Customize+.");
        DrawKeyValue("Version", PluginVersion);
        DrawKeyValue("Author", "Ruby Blaire");

        ImGui.Spacing();

        DrawSectionTitle("Safety");

        var stopOnTerritory = plugin.Configuration.StopRuntimeOnTerritoryChange;
        if (ImGui.Checkbox("Stop runtime on territory change", ref stopOnTerritory))
        {
            plugin.Configuration.StopRuntimeOnTerritoryChange = stopOnTerritory;
            plugin.SaveConfig();
        }

        var resetOnStop = plugin.Configuration.ResetLocalScaleOnStop;
        if (ImGui.Checkbox("Reset local player scale on stop", ref resetOnStop))
        {
            plugin.Configuration.ResetLocalScaleOnStop = resetOnStop;
            plugin.SaveConfig();
        }

        var panicDisablesAuto = plugin.Configuration.PanicResetDisablesAutoApply;
        if (ImGui.Checkbox("Reset disables auto-apply", ref panicDisablesAuto))
        {
            plugin.Configuration.PanicResetDisablesAutoApply = panicDisablesAuto;
            plugin.SaveConfig();
        }

        ImGui.TextColored(Muted, "Reset stops the runtime, releases the current local apply state, and records the reason for recovery tracking.");

        ImGui.Spacing();

        DrawSectionTitle("Profile-only Guard");

        var allowTemplateApply = plugin.Configuration.AllowStandaloneTemplateApply;
        if (ImGui.Checkbox("Advanced: allow standalone template apply / override modes", ref allowTemplateApply))
        {
            plugin.Configuration.AllowStandaloneTemplateApply = allowTemplateApply;

            if (!allowTemplateApply)
                plugin.Configuration.PayloadBuildMode = PayloadBuildMode.ProfileOnly;

            plugin.SaveConfig();
        }

        ImGui.TextColored(Muted, "Normal mode only applies profiles. Templates are read internally when profiles reference them.");

        ImGui.Spacing();

        DrawSectionTitle("Experimental C+ Accuracy");

        var childPropagation = plugin.Configuration.EnableExperimentalChildPropagation;
        if (ImGui.Checkbox("Enable experimental child propagation", ref childPropagation))
        {
            plugin.Configuration.EnableExperimentalChildPropagation = childPropagation;
            plugin.SaveConfig();
        }

        var propTranslation = plugin.Configuration.EnablePropagationTranslation;
        if (ImGui.Checkbox("Propagate translation", ref propTranslation))
        {
            plugin.Configuration.EnablePropagationTranslation = propTranslation;
            plugin.SaveConfig();
        }

        var propRotation = plugin.Configuration.EnablePropagationRotation;
        if (ImGui.Checkbox("Propagate rotation", ref propRotation))
        {
            plugin.Configuration.EnablePropagationRotation = propRotation;
            plugin.SaveConfig();
        }

        var propScale = plugin.Configuration.EnablePropagationScale;
        if (ImGui.Checkbox("Propagate scale / child scaling", ref propScale))
        {
            plugin.Configuration.EnablePropagationScale = propScale;
            plugin.SaveConfig();
        }

        var footerHeight = 104f;
        var remaining = ImGui.GetContentRegionAvail().Y;
        if (remaining > footerHeight)
            ImGui.Dummy(new Vector2(1f, remaining - footerHeight));

        ImGui.Separator();
        ImGui.TextColored(Muted, "Support & bug reports");

        if (ImGui.Button("Join Discord", new Vector2(160, 32)))
            OpenExternalUrl("https://discord.gg/Dr836dmbqh");

        ImGui.SameLine();

        if (ImGui.Button("Support on Ko-fi", new Vector2(160, 32)))
            OpenExternalUrl("https://ko-fi.com/rubyblaire");

        ImGui.Spacing();
        ImGui.TextColored(Muted, $"BoneSmith v{PluginVersion} • A Ruby Blaire Plugin");

        ImGui.EndChild();
    }

    private void DrawAdvancedDiagnostics()
    {
        var payload = plugin.RuntimePayloadService.LastPayload;

        DrawKeyValue("Render hook enabled", plugin.RuntimeApplyService.IsRenderHookEnabled().ToString());
        DrawKeyValue("Render hook failed", plugin.RuntimeApplyService.RenderHookFailed().ToString());
        DrawKeyValue("Render hook exceptions", plugin.RuntimeApplyService.RenderHookExceptions().ToString());
        DrawKeyValue("Render ticks", plugin.RuntimeApplyService.TickCount.ToString());
        DrawKeyValue("Render hook status", plugin.RuntimeApplyService.GetRenderHookStatus());

        ImGui.Spacing();

        DrawKeyValue("Apply enabled", plugin.LocalPlayerApplyService.IsEnabled.ToString());
        DrawKeyValue("Apply ticks", plugin.LocalPlayerApplyService.ApplyTicks.ToString());
        DrawKeyValue("Partial skeletons", plugin.LocalPlayerApplyService.LastPartialSkeletons.ToString());
        DrawKeyValue("Scanned bones", plugin.LocalPlayerApplyService.LastScannedBones.ToString());
        DrawKeyValue("Matched bones", plugin.LocalPlayerApplyService.LastMatchedBones.ToString());
        DrawKeyValue("Applied bones", plugin.LocalPlayerApplyService.LastAppliedBones.ToString());
        DrawKeyValue("Propagation source bones", plugin.LocalPlayerApplyService.LastPropagationSourceBones.ToString());
        DrawKeyValue("Propagated children", plugin.LocalPlayerApplyService.LastPropagatedChildren.ToString());

        ImGui.Spacing();

        DrawPathValue("Payload path", plugin.Configuration.LastRuntimePayloadPath ?? "(none)");
        DrawKeyValue("Parsed templates", payload?.ParsedTemplates.Count.ToString() ?? "0");
        DrawKeyValue("Final bone bindings", payload?.BoneBindings.Count.ToString() ?? "0");
        DrawKeyValue("Missing templates", payload?.MissingTemplateIds.Count.ToString() ?? "0");
        DrawKeyValue("Disabled templates skipped", payload?.DisabledTemplateIds.Count.ToString() ?? "0");

        if (payload?.ParsedTemplates.Count > 0 && ImGui.CollapsingHeader("Resolved templates"))
        {
            foreach (var template in payload.ParsedTemplates)
            {
                ImGui.Bullet();
                ImGui.SameLine();
                ImGui.TextUnformatted($"{template.TemplateName} [{template.ParsedBoneCount} bones]");
            }
        }
    }

    private void ApplyCurrentSelection()
    {
        plugin.Configuration.PayloadBuildMode = PayloadBuildMode.ProfileOnly;
        plugin.SaveConfig();
        plugin.RuntimeApplyService.ApplyCurrentSelection();
    }

    private static void OpenExternalUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true,
            });
        }
        catch (Exception ex)
        {
            Plugin.Log.Error(ex, $"Failed to open external URL: {url}");
        }
    }

    private static void DrawTooltip(string text)
    {
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(text);
    }

    private static void DrawSectionTitle(string title)
    {
        ImGui.TextColored(EmberHover, title);
        ImGui.Separator();
    }

    private static void DrawEmptyState(string title, string body)
    {
        ImGui.BeginChild("EmptyState", new Vector2(0, 120), true);
        ImGui.TextColored(EmberHover, title);
        ImGui.TextWrapped(body);
        ImGui.EndChild();
    }

    private static void DrawDetectionRow(string key, bool ok, string value)
    {
        DrawBadge(ok ? "Found" : "Missing", ok ? Good : Bad);
        ImGui.SameLine();
        ImGui.TextColored(Muted, $"{key}:");
        ImGui.SameLine();
        ImGui.TextWrapped(value);
    }

    private static void DrawBadge(string text, Vector4 color)
    {
        ImGui.TextColored(color, $"[{text}]");
    }

    private static bool DrawEmberButton(string label, Vector2 size)
    {
        ImGui.PushStyleColor(ImGuiCol.Button, EmberActive);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, EmberHover);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, EmberActive);
        var clicked = ImGui.Button(label, size);
        ImGui.PopStyleColor(3);
        return clicked;
    }

    private static void DrawKeyValue(string key, string value)
    {
        ImGui.TextColored(Muted, $"{key}:");
        ImGui.SameLine();
        ImGui.TextUnformatted(value);
    }

    private static void DrawPathValue(string key, string value)
    {
        ImGui.TextColored(Muted, $"{key}:");
        ImGui.SameLine();
        ImGui.TextWrapped(value);
    }
}
