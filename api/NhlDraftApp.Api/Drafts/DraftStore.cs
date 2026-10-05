using System.Text.Json;

namespace NhlDraftApp.Api.Drafts;

public class DraftStore
{
    public static readonly string DefaultPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NhlDraftApp", "draft.json");

    private readonly string path;
    private readonly Lock gate = new();
    private readonly Draft draft;

    public DraftStore(string path)
    {
        this.path = path;
        draft = File.Exists(path)
            ? new Draft(JsonSerializer.Deserialize<DraftData>(File.ReadAllText(path), JsonSerializerOptions.Web))
            : new Draft();
    }

    public T Read<T>(Func<Draft, T> read)
    {
        lock (gate)
            return read(draft);
    }

    public Change Apply(Func<Draft, Change> change)
    {
        lock (gate)
        {
            var result = change(draft);
            if (result.Status == ChangeStatus.Ok)
                Save();
            return result;
        }
    }

    // Write-then-replace so a crash mid-write never leaves a truncated draft.
    private void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temp = path + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(draft.ToData(), JsonSerializerOptions.Web));
        File.Move(temp, path, overwrite: true);
    }
}
