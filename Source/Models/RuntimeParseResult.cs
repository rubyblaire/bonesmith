namespace BoneSmith.Models;

public sealed class RuntimeParseResult
{
    public string? ProfileId { get; init; }
    public string? ProfileName { get; init; }
    public string? ProfilePath { get; init; }

    public List<RuntimeTemplateBinding> Templates { get; init; } = [];
    public List<string> MissingTemplateIds { get; init; } = [];
    public List<string> DisabledTemplateIds { get; init; } = [];

    public Dictionary<string, RuntimeBoneTransform> BoneBindings { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public bool HasRootTransform => BoneBindings.ContainsKey("n_root");
}
