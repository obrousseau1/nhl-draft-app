using System.Runtime.InteropServices;
using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Drafts;

public class Draft(DraftData? data = null)
{
    public const int MaxNameLength = 40;

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
        if (settings.MaxPoolers < poolers.Count)
            return Change.Invalid($"Maximum poolers cannot be below the {poolers.Count} poolers already added.");
        Settings = settings;
        return Change.Ok;
    }

    public Change AddPooler(string name)
    {
        if (Started)
            return Change.Locked;
        if (poolers.Count >= Settings.MaxPoolers)
            return Change.Invalid($"Cannot add {name.Trim()}: the pooler limit of {Settings.MaxPoolers} is reached.");
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
        if (trimmed.Length > MaxNameLength)
            return $"Pooler name cannot be longer than {MaxNameLength} characters.";
        var existing = poolers.Find(p => p.Id != self && string.Equals(p.Name, trimmed, StringComparison.OrdinalIgnoreCase));
        return existing is null ? null : $"{existing.Name} is already in the pooler list.";
    }
}
