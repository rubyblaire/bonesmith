using BoneSmith.Models;
using Dalamud.Configuration;

namespace BoneSmith;

public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 3;

    public bool AutoDetectCustomizePlus { get; set; } = true;
    public bool CompatibilityRuntimeEnabled { get; set; } = false;
    public bool StopRuntimeOnTerritoryChange { get; set; } = true;
    public bool ResetLocalScaleOnStop { get; set; } = true;
    public bool ConfirmedExperimentalApplyWarning { get; set; } = false;
    public bool AutoBuildPayloadOnApply { get; set; } = true;
    public bool EnableExperimentalChildPropagation { get; set; } = false;
    public bool EnablePropagationTranslation { get; set; } = true;
    public bool EnablePropagationRotation { get; set; } = true;
    public bool EnablePropagationScale { get; set; } = true;

    public bool RememberLastSelection { get; set; } = true;
    public bool AllowStandaloneTemplateApply { get; set; } = false;
    public bool AutoApplyLastSelectionOnLogin { get; set; } = false;
    public bool RequireManualConfirmForAutoApply { get; set; } = true;
    public bool PanicResetDisablesAutoApply { get; set; } = true;

    public DateTimeOffset? LastApplyStartedAt { get; set; }
    public DateTimeOffset? LastApplyStoppedAt { get; set; }
    public string? LastAppliedProfileName { get; set; }
    public string? LastAppliedTemplateName { get; set; }
    public string? LastAppliedPayloadMode { get; set; }
    public string? LastPanicResetReason { get; set; }
    public PayloadBuildMode PayloadBuildMode { get; set; } = PayloadBuildMode.ProfileOnly;

    public string? LastDetectedCustomizePlusConfigPath { get; set; }
    public string? LastDetectedCustomizePlusDataFolder { get; set; }
    public string? LastDetectedCustomizePlusProfilesFolder { get; set; }
    public string? LastDetectedCustomizePlusTemplatesFolder { get; set; }

    public DateTimeOffset? LastBackupCreatedAt { get; set; }

    public string? ActiveProfileId { get; set; }
    public string? ActiveProfileName { get; set; }
    public string? ActiveProfilePath { get; set; }

    public string? ActiveTemplateId { get; set; }
    public string? ActiveTemplateName { get; set; }
    public string? ActiveTemplatePath { get; set; }

    public string? SelectedProfilePath
    {
        get => ActiveProfilePath;
        set => ActiveProfilePath = value;
    }

    public string? SelectedTemplatePath
    {
        get => ActiveTemplatePath;
        set => ActiveTemplatePath = value;
    }

    public string? SelectedProfileName
    {
        get => ActiveProfileName;
        set => ActiveProfileName = value;
    }

    public string? SelectedTemplateName
    {
        get => ActiveTemplateName;
        set => ActiveTemplateName = value;
    }

    public string? LastRuntimePayloadPath { get; set; }
    public DateTimeOffset? LastRuntimePayloadBuiltAt { get; set; }
    public string? LastRuntimeStatus { get; set; }

    public bool WriteBackToCustomizePlus { get; set; } = false;


    public Dictionary<string, Dictionary<string, bool>> TemplateEnabledOverridesByProfileId { get; set; } = [];

    public bool GetTemplateEnabledForProfile(string? profileId, string? profilePath, string templateId, bool defaultEnabled)
    {
        var key = GetProfileTemplateOverrideKey(profileId, profilePath);

        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(templateId))
            return defaultEnabled;

        return TemplateEnabledOverridesByProfileId is not null &&
               TemplateEnabledOverridesByProfileId.TryGetValue(key, out var overrides) &&
               overrides.TryGetValue(templateId, out var enabled)
            ? enabled
            : defaultEnabled;
    }

    public void SetTemplateEnabledForProfile(string? profileId, string? profilePath, string templateId, bool enabled)
    {
        var key = GetProfileTemplateOverrideKey(profileId, profilePath);

        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(templateId))
            return;

        TemplateEnabledOverridesByProfileId ??= [];

        if (!TemplateEnabledOverridesByProfileId.TryGetValue(key, out var overrides))
        {
            overrides = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            TemplateEnabledOverridesByProfileId[key] = overrides;
        }

        overrides[templateId] = enabled;
    }

    public void ClearTemplateOverridesForProfile(string? profileId, string? profilePath)
    {
        var key = GetProfileTemplateOverrideKey(profileId, profilePath);

        if (string.IsNullOrWhiteSpace(key))
            return;

        TemplateEnabledOverridesByProfileId?.Remove(key);
    }

    public static string? GetProfileTemplateOverrideKey(string? profileId, string? profilePath)
    {
        if (!string.IsNullOrWhiteSpace(profileId))
            return profileId;

        return string.IsNullOrWhiteSpace(profilePath) ? null : profilePath;
    }

}
