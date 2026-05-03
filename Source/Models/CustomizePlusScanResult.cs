namespace BoneSmith.Models;

public sealed class CustomizePlusScanResult
{
    public bool FoundConfig { get; init; }
    public bool FoundDataFolder { get; init; }
    public bool FoundProfilesFolder { get; init; }
    public bool FoundTemplatesFolder { get; init; }

    public string? ConfigPath { get; init; }
    public string? DataFolderPath { get; init; }
    public string? ProfilesFolderPath { get; init; }
    public string? TemplatesFolderPath { get; init; }

    public string? ProfileSortOrderPath { get; init; }
    public string? TemplateSortOrderPath { get; init; }

    public string? Error { get; init; }

    public List<string> TopLevelConfigKeys { get; init; } = [];
    public List<LocalJsonFileItem> Profiles { get; init; } = [];
    public List<LocalJsonFileItem> Templates { get; init; } = [];
}
