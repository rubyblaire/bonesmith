namespace BoneSmith.Models;

public sealed class RuntimeTemplateBinding
{
    public string TemplateId { get; init; } = string.Empty;
    public string TemplateName { get; init; } = string.Empty;
    public string TemplatePath { get; init; } = string.Empty;
    public bool EnabledInProfile { get; init; } = true;
    public int ParsedBoneCount { get; init; }
}
