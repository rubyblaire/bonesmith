using System.Text.Json;
using BoneSmith.Models;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace BoneSmith.Services;

public sealed class RuntimePayloadService
{
    private readonly Configuration configuration;
    private readonly IDalamudPluginInterface pluginInterface;
    private readonly IPluginLog log;
    private readonly CustomizePlusRuntimeParser parser;

    public RuntimePayload? LastPayload { get; private set; }

    public RuntimePayloadService(
        Configuration configuration,
        IDalamudPluginInterface pluginInterface,
        IPluginLog log,
        CustomizePlusRuntimeParser parser)
    {
        this.configuration = configuration;
        this.pluginInterface = pluginInterface;
        this.log = log;
        this.parser = parser;
    }

    public RuntimePayload? BuildPayload()
    {
        var parseResult = parser.ParseActive(configuration);

        if (parseResult.BoneBindings.Count == 0 && parseResult.Templates.Count == 0)
            return null;

        var candidateBones = parseResult.BoneBindings.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();

        var payload = new RuntimePayload
        {
            ProfileId = parseResult.ProfileId,
            ProfileName = parseResult.ProfileName,
            ProfilePath = parseResult.ProfilePath,
            TemplateId = configuration.ActiveTemplateId,
            TemplateName = configuration.ActiveTemplateName,
            TemplatePath = configuration.ActiveTemplatePath,
            TransformLikeNodeCount = parseResult.BoneBindings.Count,
            CandidateBoneNames = candidateBones,
            LinkedTemplateIds = parseResult.Templates.Select(x => x.TemplateId).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            ParsedTemplates = parseResult.Templates,
            BoneBindings = parseResult.BoneBindings,
            MissingTemplateIds = parseResult.MissingTemplateIds,
            DisabledTemplateIds = parseResult.DisabledTemplateIds,
            HasRootTransform = parseResult.HasRootTransform,
            BuiltAt = DateTimeOffset.Now
        };

        var runtimeDir = Path.Combine(pluginInterface.ConfigDirectory.FullName, "Runtime");
        Directory.CreateDirectory(runtimeDir);

        var path = Path.Combine(runtimeDir, $"runtime_payload_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));

        LastPayload = payload;

        configuration.ActiveProfileId = payload.ProfileId ?? configuration.ActiveProfileId;
        configuration.LastRuntimePayloadPath = path;
        configuration.LastRuntimePayloadBuiltAt = payload.BuiltAt;
        configuration.LastRuntimeStatus = "Runtime payload built with parsed Customize+ bone bindings. Awaiting low-level apply backend.";
        pluginInterface.SavePluginConfig(configuration);

        log.Information("BoneSmith runtime payload built at {Path}", path);

        return payload;
    }
    public void ClearPayload()
    {
        LastPayload = null;
        configuration.LastRuntimePayloadPath = null;
        configuration.LastRuntimePayloadBuiltAt = null;
        configuration.LastRuntimeStatus = "Runtime payload cleared.";
        pluginInterface.SavePluginConfig(configuration);
    }

}
