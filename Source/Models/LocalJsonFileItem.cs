namespace BoneSmith.Models;

public sealed class LocalJsonFileItem
{
    public string Id { get; init; } = string.Empty;
    public string FullPath { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? FileName : Name;

    public long FileSizeBytes { get; init; }
    public DateTime LastWriteTime { get; init; }

    public int TransformLikeNodeCount { get; init; }
    public List<string> BoneNames { get; init; } = [];
    public List<string> LinkedTemplateIds { get; init; } = [];
    public List<ProfileTemplateReferenceInfo> ReferencedTemplates { get; init; } = [];

    public string JsonPreview { get; init; } = string.Empty;
}
