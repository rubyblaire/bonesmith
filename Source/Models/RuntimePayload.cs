namespace BoneSmith.Models;

public sealed class RuntimePayload
{
    public string? ProfileId { get; init; }
    public string? ProfileName { get; init; }
    public string? ProfilePath { get; init; }

    public string? TemplateId { get; init; }
    public string? TemplateName { get; init; }
    public string? TemplatePath { get; init; }

    public int TransformLikeNodeCount { get; init; }
    public List<string> CandidateBoneNames { get; init; } = [];
    public List<string> LinkedTemplateIds { get; init; } = [];

    public List<RuntimeTemplateBinding> ParsedTemplates { get; init; } = [];
    public Dictionary<string, RuntimeBoneTransform> BoneBindings { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> MissingTemplateIds { get; init; } = [];
    public List<string> DisabledTemplateIds { get; init; } = [];
    public bool HasRootTransform { get; init; }

    public DateTimeOffset BuiltAt { get; init; } = DateTimeOffset.Now;
}
