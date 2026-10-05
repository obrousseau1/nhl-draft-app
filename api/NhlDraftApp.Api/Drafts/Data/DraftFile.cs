using System.Text.Json;
using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Drafts.Data;

// One file per draft (draft-<timestamp>.json); the newest is the current draft.
public class DraftFile(string folder, ILogger<DraftFile> logger, TimeProvider time) : IDraftFile
{
    public static readonly string DefaultFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NhlDraftApp");

    private string? current;

    public DraftData? Load()
    {
        var newest = Newest();
        if (newest is null)
            return null;
        var data = Parse(File.ReadAllText(newest));
        if (data is not null)
        {
            current = newest;
            return data;
        }
        SetAsideCorrupt(newest);
        return null;
    }

    public void Save(DraftData data) => current = Write(current ?? NewPath(), data);

    public void SaveAsNew(DraftData data) => current = Write(NewPath(), data);

    private static DraftData? Parse(string json)
    {
        try
        {
            var data = JsonSerializer.Deserialize<DraftData>(json, JsonSerializerOptions.Web);
            return data is { Settings: not null, Poolers: not null, Picks: not null } ? data : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    // Write-then-replace so a crash mid-write never leaves a truncated draft.
    private static string Write(string path, DraftData data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temp = path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(data, JsonSerializerOptions.Web));
        File.Move(temp, path, overwrite: true);
        return path;
    }

    private string? Newest() =>
        Directory.Exists(folder) ? Directory.GetFiles(folder, "draft-*.json").Max(StringComparer.Ordinal) : null;

    private string NewPath() => Path.Combine(folder, $"draft-{Stamp()}.json");

    private string Stamp() => time.GetLocalNow().ToString("yyyyMMdd-HHmmss-fff");

    private void SetAsideCorrupt(string path)
    {
        var corrupt = $"{path}.corrupt-{Stamp()}";
        File.Move(path, corrupt);
        logger.LogWarning("Draft file {Path} was unreadable; moved to {Corrupt} and started an empty draft.", path, corrupt);
    }
}
