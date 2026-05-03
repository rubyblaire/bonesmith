using System.Numerics;
using System.Text.Json;
using BoneSmith.Models;

namespace BoneSmith.Services;

public sealed class CustomizePlusRuntimeParser
{
    private readonly CustomizePlusDataService dataService;

    public CustomizePlusRuntimeParser(CustomizePlusDataService dataService)
    {
        this.dataService = dataService;
    }

    public RuntimeParseResult ParseActive(Configuration configuration)
    {
        dataService.RefreshDetection();

        var mode = configuration.PayloadBuildMode;

        if (!configuration.AllowStandaloneTemplateApply)
        {
            mode = PayloadBuildMode.ProfileOnly;
        }

        if (mode == PayloadBuildMode.Auto)
        {
            mode = !string.IsNullOrWhiteSpace(configuration.ActiveProfilePath) && File.Exists(configuration.ActiveProfilePath)
                ? PayloadBuildMode.ProfileOnly
                : PayloadBuildMode.TemplateOnly;
        }

        return mode switch
        {
            PayloadBuildMode.ProfileOnly => ParseProfileIfAvailable(configuration) ?? new RuntimeParseResult(),
            PayloadBuildMode.TemplateOnly => ParseTemplateIfAvailable(configuration) ?? new RuntimeParseResult(),
            PayloadBuildMode.ProfilePlusTemplateOverride => ParseProfilePlusTemplateOverride(configuration),
            _ => new RuntimeParseResult()
        };
    }

    private RuntimeParseResult? ParseProfileIfAvailable(Configuration configuration)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ActiveProfilePath) && File.Exists(configuration.ActiveProfilePath))
            return ParseProfile(configuration.ActiveProfilePath, configuration.ActiveProfileName, configuration);

        return null;
    }

    private RuntimeParseResult? ParseTemplateIfAvailable(Configuration configuration)
    {
        if (!string.IsNullOrWhiteSpace(configuration.ActiveTemplatePath) && File.Exists(configuration.ActiveTemplatePath))
            return ParseSingleTemplate(configuration.ActiveTemplatePath, configuration.ActiveTemplateName);

        return null;
    }

    private RuntimeParseResult ParseProfilePlusTemplateOverride(Configuration configuration)
    {
        var profileResult = ParseProfileIfAvailable(configuration) ?? new RuntimeParseResult();

        if (string.IsNullOrWhiteSpace(configuration.ActiveTemplatePath) || !File.Exists(configuration.ActiveTemplatePath))
            return profileResult;

        var templateResult = ParseSingleTemplate(configuration.ActiveTemplatePath, configuration.ActiveTemplateName);

        foreach (var template in templateResult.Templates)
        {
            profileResult.Templates.Add(new RuntimeTemplateBinding
            {
                TemplateId = template.TemplateId,
                TemplateName = $"{template.TemplateName} (active override)",
                TemplatePath = template.TemplatePath,
                EnabledInProfile = true,
                ParsedBoneCount = template.ParsedBoneCount
            });
        }

        foreach (var (boneName, transform) in templateResult.BoneBindings)
        {
            if (!transform.IsEdited)
                continue;

            // Active template is applied last, so it overrides profile-linked templates for duplicate bones.
            profileResult.BoneBindings[boneName] = transform;
        }

        return profileResult;
    }

    private RuntimeParseResult ParseProfile(string profilePath, string? fallbackName, Configuration configuration)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(profilePath));
        var root = document.RootElement;

        var profileId = GetString(root, "UniqueId") ?? CustomizePlusDataService.ExtractIdFromFilePath(profilePath);
        var profileName = GetString(root, "Name") ?? fallbackName ?? profileId ?? "Profile";

        var templateRefs = ReadProfileTemplateRefs(root);

        var result = new RuntimeParseResult
        {
            ProfileId = profileId,
            ProfileName = profileName,
            ProfilePath = profilePath
        };

        foreach (var templateRef in templateRefs)
        {
            var enabled = configuration.GetTemplateEnabledForProfile(profileId, profilePath, templateRef.TemplateId, templateRef.Enabled);

            if (!enabled)
            {
                result.DisabledTemplateIds.Add(templateRef.TemplateId);
                continue;
            }

            var templateItem = dataService.LastScanResult.Templates
                .FirstOrDefault(x => string.Equals(x.Id, templateRef.TemplateId, StringComparison.OrdinalIgnoreCase));

            if (templateItem is null || !File.Exists(templateItem.FullPath))
            {
                result.MissingTemplateIds.Add(templateRef.TemplateId);
                continue;
            }

            var parsedTemplate = ParseTemplateFile(templateItem.FullPath, templateItem.DisplayName, templateItem.Id);

            result.Templates.Add(new RuntimeTemplateBinding
            {
                TemplateId = parsedTemplate.TemplateId ?? templateItem.Id,
                TemplateName = parsedTemplate.TemplateName ?? templateItem.DisplayName,
                TemplatePath = templateItem.FullPath,
                EnabledInProfile = true,
                ParsedBoneCount = parsedTemplate.Bones.Count
            });

            // Match C+ binding behavior: later templates override earlier templates for the same bone.
            foreach (var (boneName, transform) in parsedTemplate.Bones)
            {
                if (!transform.IsEdited)
                    continue;

                result.BoneBindings[boneName] = transform;
            }
        }

        return result;
    }

    private RuntimeParseResult ParseSingleTemplate(string templatePath, string? fallbackName)
    {
        var parsedTemplate = ParseTemplateFile(templatePath, fallbackName, CustomizePlusDataService.ExtractIdFromFilePath(templatePath));

        var result = new RuntimeParseResult
        {
            ProfileId = null,
            ProfileName = "(single template mode)",
            ProfilePath = null
        };

        result.Templates.Add(new RuntimeTemplateBinding
        {
            TemplateId = parsedTemplate.TemplateId ?? string.Empty,
            TemplateName = parsedTemplate.TemplateName ?? fallbackName ?? Path.GetFileNameWithoutExtension(templatePath),
            TemplatePath = templatePath,
            EnabledInProfile = true,
            ParsedBoneCount = parsedTemplate.Bones.Count
        });

        foreach (var (boneName, transform) in parsedTemplate.Bones)
        {
            if (!transform.IsEdited)
                continue;

            result.BoneBindings[boneName] = transform;
        }

        return result;
    }

    private ParsedTemplate ParseTemplateFile(string templatePath, string? fallbackName, string? fallbackId)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(templatePath));
        var root = document.RootElement;

        var templateId = GetString(root, "UniqueId") ?? fallbackId;
        var templateName = GetString(root, "Name") ?? fallbackName ?? templateId ?? "Template";
        var bones = new Dictionary<string, RuntimeBoneTransform>(StringComparer.OrdinalIgnoreCase);

        if (root.TryGetProperty("Bones", out var bonesElement) && bonesElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var boneProperty in bonesElement.EnumerateObject())
            {
                if (boneProperty.Value.ValueKind != JsonValueKind.Object)
                    continue;

                var transform = ParseBoneTransform(boneProperty.Name, boneProperty.Value);

                if (transform.IsEdited)
                    bones[boneProperty.Name] = transform;
            }
        }

        return new ParsedTemplate
        {
            TemplateId = templateId,
            TemplateName = templateName,
            Bones = bones
        };
    }

    private static RuntimeBoneTransform ParseBoneTransform(string boneName, JsonElement element)
    {
        return new RuntimeBoneTransform
        {
            BoneName = boneName,
            Translation = GetVector3(element, "Translation", Vector3.Zero),
            Rotation = GetVector3(element, "Rotation", Vector3.Zero),
            Scaling = GetVector3(element, "Scaling", Vector3.One),
            ChildScaling = GetVector3(element, "ChildScaling", Vector3.One),
            PropagateTranslation = GetBool(element, "PropagateTranslation"),
            PropagateRotation = GetBool(element, "PropagateRotation"),
            PropagateScale = GetBool(element, "PropagateScale"),
            ChildScalingIndependent = GetBool(element, "ChildScalingIndependent")
        };
    }

    private static List<ProfileTemplateRef> ReadProfileTemplateRefs(JsonElement profileRoot)
    {
        var refs = new List<ProfileTemplateRef>();

        if (!profileRoot.TryGetProperty("Templates", out var templatesElement) ||
            templatesElement.ValueKind != JsonValueKind.Array)
        {
            return refs;
        }

        foreach (var item in templatesElement.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
                continue;

            var id = GetString(item, "TemplateId");

            if (string.IsNullOrWhiteSpace(id))
                continue;

            refs.Add(new ProfileTemplateRef
            {
                TemplateId = id,
                Enabled = !item.TryGetProperty("Enabled", out var enabledElement) ||
                          enabledElement.ValueKind != JsonValueKind.False
            });
        }

        return refs;
    }

    private static Vector3 GetVector3(JsonElement element, string propertyName, Vector3 fallback)
    {
        if (!element.TryGetProperty(propertyName, out var value))
            return fallback;

        if (value.ValueKind == JsonValueKind.Object)
        {
            var x = GetFloat(value, "X", fallback.X);
            var y = GetFloat(value, "Y", fallback.Y);
            var z = GetFloat(value, "Z", fallback.Z);
            return ClampVector(new Vector3(x, y, z));
        }

        if (value.ValueKind == JsonValueKind.Array)
        {
            var values = value.EnumerateArray().Take(3).Select(x => x.TryGetSingle(out var f) ? f : 0f).ToArray();
            if (values.Length >= 3)
                return ClampVector(new Vector3(values[0], values[1], values[2]));
        }

        return fallback;
    }

    private static float GetFloat(JsonElement element, string propertyName, float fallback)
    {
        if (!element.TryGetProperty(propertyName, out var value))
            return fallback;

        if (value.TryGetSingle(out var single))
            return single;

        if (value.TryGetDouble(out var dbl))
            return (float)dbl;

        return fallback;
    }

    private static bool GetBool(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var value) &&
               value.ValueKind == JsonValueKind.True;
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
            return null;

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            _ => value.ToString()
        };
    }

    private static Vector3 ClampVector(Vector3 vector)
    {
        return new Vector3(
            Math.Clamp(vector.X, -512f, 512f),
            Math.Clamp(vector.Y, -512f, 512f),
            Math.Clamp(vector.Z, -512f, 512f));
    }

    private sealed class ProfileTemplateRef
    {
        public string TemplateId { get; init; } = string.Empty;
        public bool Enabled { get; init; }
    }

    private sealed class ParsedTemplate
    {
        public string? TemplateId { get; init; }
        public string? TemplateName { get; init; }
        public Dictionary<string, RuntimeBoneTransform> Bones { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
