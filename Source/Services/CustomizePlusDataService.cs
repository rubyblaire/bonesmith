using System.Text.Json;
using BoneSmith.Models;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace BoneSmith.Services;

public sealed class CustomizePlusDataService
{
    private readonly Configuration configuration;
    private readonly IDalamudPluginInterface pluginInterface;
    private readonly IPluginLog log;

    public CustomizePlusScanResult LastScanResult { get; private set; } = new()
    {
        Error = "Not scanned yet."
    };

    public CustomizePlusDataService(Configuration configuration, IDalamudPluginInterface pluginInterface, IPluginLog log)
    {
        this.configuration = configuration;
        this.pluginInterface = pluginInterface;
        this.log = log;
    }

    public CustomizePlusScanResult RefreshDetection()
    {
        try
        {
            var configPath = GetDefaultCustomizePlusConfigPath();
            var dataFolder = GetDefaultCustomizePlusDataFolder();
            var profilesFolder = Path.Combine(dataFolder, "profiles");
            var templatesFolder = Path.Combine(dataFolder, "templates");

            var topLevelKeys = ReadTopLevelKeys(configPath);
            var profiles = ScanJsonFolder(profilesFolder);
            var templates = ScanJsonFolder(templatesFolder);

            var profileSortPath = Path.Combine(dataFolder, "profile_sort_order.json");
            var templateSortPath = Path.Combine(dataFolder, "template_sort_order.json");
            var templateSortBackupPath = Path.Combine(dataFolder, "template_sort_order.json.bak");

            configuration.LastDetectedCustomizePlusConfigPath = File.Exists(configPath) ? configPath : null;
            configuration.LastDetectedCustomizePlusDataFolder = Directory.Exists(dataFolder) ? dataFolder : null;
            configuration.LastDetectedCustomizePlusProfilesFolder = Directory.Exists(profilesFolder) ? profilesFolder : null;
            configuration.LastDetectedCustomizePlusTemplatesFolder = Directory.Exists(templatesFolder) ? templatesFolder : null;

            LastScanResult = new CustomizePlusScanResult
            {
                FoundConfig = File.Exists(configPath),
                FoundDataFolder = Directory.Exists(dataFolder),
                FoundProfilesFolder = Directory.Exists(profilesFolder),
                FoundTemplatesFolder = Directory.Exists(templatesFolder),
                ConfigPath = configPath,
                DataFolderPath = dataFolder,
                ProfilesFolderPath = profilesFolder,
                TemplatesFolderPath = templatesFolder,
                ProfileSortOrderPath = File.Exists(profileSortPath) ? profileSortPath : null,
                TemplateSortOrderPath = File.Exists(templateSortPath) ? templateSortPath : File.Exists(templateSortBackupPath) ? templateSortBackupPath : null,
                TopLevelConfigKeys = topLevelKeys,
                Profiles = profiles,
                Templates = templates
            };

            return LastScanResult;
        }
        catch (Exception ex)
        {
            log.Error(ex, "Failed to scan Customize+ data.");

            LastScanResult = new CustomizePlusScanResult
            {
                Error = ex.Message,
                ConfigPath = GetDefaultCustomizePlusConfigPath(),
                DataFolderPath = GetDefaultCustomizePlusDataFolder()
            };

            return LastScanResult;
        }
    }

    public string? CreateFullBackup()
    {
        RefreshDetection();

        var backupRoot = Path.Combine(pluginInterface.ConfigDirectory.FullName, "Backups", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        Directory.CreateDirectory(backupRoot);

        var copiedAny = false;

        if (File.Exists(LastScanResult.ConfigPath))
        {
            File.Copy(LastScanResult.ConfigPath, Path.Combine(backupRoot, "CustomizePlus.json"), overwrite: false);
            copiedAny = true;
        }

        if (Directory.Exists(LastScanResult.DataFolderPath))
        {
            var dataBackup = Path.Combine(backupRoot, "CustomizePlus");
            CopyDirectory(LastScanResult.DataFolderPath!, dataBackup);
            copiedAny = true;
        }

        if (!copiedAny)
            return null;

        configuration.LastBackupCreatedAt = DateTimeOffset.Now;
        pluginInterface.SavePluginConfig(configuration);

        return backupRoot;
    }

    public void UseProfile(LocalJsonFileItem item)
    {
        configuration.ActiveProfileId = item.Id;
        configuration.ActiveProfileName = item.DisplayName;
        configuration.ActiveProfilePath = item.FullPath;
        pluginInterface.SavePluginConfig(configuration);
    }

    public void UseTemplate(LocalJsonFileItem item)
    {
        configuration.ActiveTemplateId = item.Id;
        configuration.ActiveTemplateName = item.DisplayName;
        configuration.ActiveTemplatePath = item.FullPath;
        pluginInterface.SavePluginConfig(configuration);
    }

    private static List<string> ReadTopLevelKeys(string path)
    {
        if (!File.Exists(path))
            return [];

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                return [];

            return document.RootElement.EnumerateObject().Select(p => p.Name).OrderBy(x => x).ToList();
        }
        catch
        {
            return [];
        }
    }

    private static List<LocalJsonFileItem> ScanJsonFolder(string folder)
    {
        if (!Directory.Exists(folder))
            return [];

        var items = new List<LocalJsonFileItem>();

        foreach (var file in Directory.EnumerateFiles(folder, "*.json", SearchOption.TopDirectoryOnly))
        {
            var info = new FileInfo(file);
            var json = SafeReadAllText(file);
            var id = ExtractUniqueId(json) ?? ExtractIdFromFilePath(file) ?? Path.GetFileNameWithoutExtension(file);
            var name = ExtractDisplayName(json) ?? id;
            var referencedTemplates = ExtractProfileTemplateRefs(json);
            var linkedTemplateIds = referencedTemplates.Count > 0
                ? referencedTemplates.Select(x => x.TemplateId).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                : ExtractGuids(json);

            items.Add(new LocalJsonFileItem
            {
                Id = id,
                FullPath = file,
                FileName = Path.GetFileName(file),
                Name = name,
                FileSizeBytes = info.Length,
                LastWriteTime = info.LastWriteTime,
                TransformLikeNodeCount = CountLikelyTransformNodes(json),
                BoneNames = ExtractBoneNames(json, 200),
                LinkedTemplateIds = linkedTemplateIds,
                ReferencedTemplates = referencedTemplates,
                JsonPreview = json.Length <= 4000 ? json : json[..4000] + "\n..."
            });
        }

        return items
            .OrderBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.FileName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string SafeReadAllText(string path)
    {
        try
        {
            return File.ReadAllText(path);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string? ExtractUniqueId(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
                return null;

            foreach (var key in new[] { "UniqueId", "uniqueId", "Id", "id" })
            {
                if (document.RootElement.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String)
                    return value.GetString();
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    private static List<ProfileTemplateReferenceInfo> ExtractProfileTemplateRefs(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty("Templates", out var templatesElement) ||
                templatesElement.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            var refs = new List<ProfileTemplateReferenceInfo>();

            foreach (var item in templatesElement.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                    continue;

                if (!item.TryGetProperty("TemplateId", out var idElement))
                    continue;

                var id = idElement.ValueKind == JsonValueKind.String ? idElement.GetString() : idElement.ToString();

                if (string.IsNullOrWhiteSpace(id))
                    continue;

                refs.Add(new ProfileTemplateReferenceInfo
                {
                    TemplateId = id,
                    EnabledInProfile = !item.TryGetProperty("Enabled", out var enabledElement) ||
                                       enabledElement.ValueKind != JsonValueKind.False
                });
            }

            return refs;
        }
        catch
        {
            return [];
        }
    }

    private static string? ExtractDisplayName(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using var document = JsonDocument.Parse(json);
            return ExtractDisplayName(document.RootElement);
        }
        catch
        {
            return null;
        }
    }

    private static string? ExtractDisplayName(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return null;

        foreach (var key in new[] { "Name", "name", "DisplayName", "displayName", "Label", "label" })
        {
            if (element.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String)
                return value.GetString();
        }

        foreach (var property in element.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                var nested = ExtractDisplayName(property.Value);
                if (!string.IsNullOrWhiteSpace(nested))
                    return nested;
            }
        }

        return null;
    }

    public static int CountLikelyTransformNodes(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return 0;

        try
        {
            using var document = JsonDocument.Parse(json);
            return CountLikelyTransformNodes(document.RootElement);
        }
        catch
        {
            return 0;
        }
    }

    public static int CountLikelyTransformNodes(JsonElement element)
    {
        var count = 0;

        void Walk(JsonElement current)
        {
            switch (current.ValueKind)
            {
                case JsonValueKind.Object:
                    var hasTransformishKey = false;

                    foreach (var property in current.EnumerateObject())
                    {
                        var name = property.Name;

                        if (name.Contains("Scale", StringComparison.OrdinalIgnoreCase) ||
                            name.Contains("Position", StringComparison.OrdinalIgnoreCase) ||
                            name.Contains("Rotation", StringComparison.OrdinalIgnoreCase) ||
                            name.Contains("Translation", StringComparison.OrdinalIgnoreCase) ||
                            name.Contains("Transform", StringComparison.OrdinalIgnoreCase))
                        {
                            hasTransformishKey = true;
                        }

                        Walk(property.Value);
                    }

                    if (hasTransformishKey)
                        count++;

                    break;

                case JsonValueKind.Array:
                    foreach (var item in current.EnumerateArray())
                        Walk(item);
                    break;
            }
        }

        Walk(element);
        return count;
    }

    public static List<string> ExtractBoneNames(string json, int maxResults = 200)
    {
        var results = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            using var document = JsonDocument.Parse(json);
            ExtractBoneNames(document.RootElement, results, maxResults);
        }
        catch
        {
            return [];
        }

        return results.Take(maxResults).ToList();
    }

    private static void ExtractBoneNames(JsonElement element, SortedSet<string> results, int maxResults)
    {
        if (results.Count >= maxResults)
            return;

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    if (results.Count >= maxResults)
                        return;

                    var key = property.Name;

                    if ((key.Equals("BoneName", StringComparison.OrdinalIgnoreCase) ||
                         key.Equals("Bone", StringComparison.OrdinalIgnoreCase) ||
                         key.Equals("Node", StringComparison.OrdinalIgnoreCase)) &&
                        property.Value.ValueKind == JsonValueKind.String)
                    {
                        var value = property.Value.GetString();
                        if (!string.IsNullOrWhiteSpace(value) && LooksLikeBoneName(value))
                            results.Add(value);
                    }

                    ExtractBoneNames(property.Value, results, maxResults);
                }

                break;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                    ExtractBoneNames(item, results, maxResults);
                break;
        }
    }

    private static bool LooksLikeBoneName(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 128)
            return false;

        return value.Contains("j_", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("n_", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("_", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("bone", StringComparison.OrdinalIgnoreCase);
    }

    public static string? ExtractIdFromFilePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        var fileName = Path.GetFileNameWithoutExtension(path);
        return Guid.TryParse(fileName, out _) ? fileName : null;
    }

    public static List<string> ExtractGuids(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        var matches = System.Text.RegularExpressions.Regex.Matches(
            json,
            @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");

        return matches
            .Select(m => m.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string GetDefaultCustomizePlusConfigPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "XIVLauncher", "pluginConfigs", "CustomizePlus.json");
    }

    private static string GetDefaultCustomizePlusDataFolder()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "XIVLauncher", "pluginConfigs", "CustomizePlus");
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        Directory.CreateDirectory(destinationDir);

        foreach (var file in Directory.GetFiles(sourceDir))
            File.Copy(file, Path.Combine(destinationDir, Path.GetFileName(file)), overwrite: false);

        foreach (var directory in Directory.GetDirectories(sourceDir))
            CopyDirectory(directory, Path.Combine(destinationDir, Path.GetFileName(directory)));
    }
}
