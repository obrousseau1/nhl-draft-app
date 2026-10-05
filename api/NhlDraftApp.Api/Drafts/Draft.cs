using System.Runtime.InteropServices;

namespace NhlDraftApp.Api.Drafts;

public record Pooler(Guid Id, string Name);

public record Pick(Guid PoolerId, int Round, string EntryId);

public record DraftData(Settings Settings, List<Pooler> Poolers, List<Pick> Picks);

public class Draft(DraftData? data = null)
{
    private readonly List<Pooler> poolers = data?.Poolers.ToList() ?? [];
    private readonly List<Pick> picks = data?.Picks.ToList() ?? [];

    public Settings Settings { get; private set; } = data?.Settings ?? new();
    public IReadOnlyList<Pooler> Poolers => poolers;
    public IReadOnlyList<Pick> Picks => picks;
    public bool Started => picks.Count > 0;

    public DraftData ToData() => new(Settings, [.. poolers], [.. picks]);

    public Change UpdateSettings(Settings settings)
    {
        if (Started)
            return Change.Locked;
        if (settings.Problem() is { } problem)
            return Change.Invalid(problem);
        Settings = settings;
        return Change.Ok;
    }

    public Change AddPooler(string name)
    {
        if (Started)
            return Change.Locked;
        if (NameProblem(name, null) is { } problem)
            return Change.Invalid(problem);
        poolers.Add(new Pooler(Guid.NewGuid(), name.Trim()));
        return Change.Ok;
    }

    public Change RenamePooler(Guid id, string name)
    {
        var index = poolers.FindIndex(p => p.Id == id);
        if (index < 0)
            return Change.NotFound;
        if (NameProblem(name, id) is { } problem)
            return Change.Invalid(problem);
        poolers[index] = poolers[index] with { Name = name.Trim() };
        return Change.Ok;
    }

    public Change RemovePooler(Guid id)
    {
        if (Started)
            return Change.Locked;
        return poolers.RemoveAll(p => p.Id == id) > 0 ? Change.Ok : Change.NotFound;
    }

    public Change Reorder(IReadOnlyList<Guid> ids)
    {
        if (Started)
            return Change.Locked;
        if (!ListsEveryPoolerOnce(ids))
            return Change.Invalid("The order must list every pooler exactly once.");
        var byId = poolers.ToDictionary(p => p.Id);
        poolers.Clear();
        poolers.AddRange(ids.Select(id => byId[id]));
        return Change.Ok;
    }

    public Change Shuffle(Random random)
    {
        if (Started)
            return Change.Locked;
        random.Shuffle(CollectionsMarshal.AsSpan(poolers));
        return Change.Ok;
    }

    public Change Reset()
    {
        picks.Clear();
        return Change.Ok;
    }

    private bool ListsEveryPoolerOnce(IReadOnlyList<Guid> ids) =>
        ids.Count == poolers.Count && poolers.All(p => ids.Contains(p.Id));

    private string? NameProblem(string name, Guid? self)
    {
        var trimmed = name.Trim();
        if (trimmed.Length == 0)
            return "Pooler name cannot be empty.";
        if (poolers.Any(p => p.Id != self && string.Equals(p.Name, trimmed, StringComparison.OrdinalIgnoreCase)))
            return $"A pooler named {trimmed} already exists.";
        return null;
    }
}
