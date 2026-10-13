using System.Runtime.InteropServices;
using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Drafts;

public class Draft(DraftData? data = null)
{
    public const int MaxNameLength = 40;

    private readonly List<Pooler> poolers = data?.Poolers.ToList() ?? [];
    private readonly List<Pick> picks = data?.Picks.ToList() ?? [];
    private readonly List<PoolEntry> pool = data?.Pool?.ToList() ?? [];

    public Settings Settings { get; private set; } = data?.Settings ?? new();
    public IReadOnlyList<Pooler> Poolers => poolers;
    public IReadOnlyList<Pick> Picks => picks;
    public bool Started => picks.Count > 0;

    public DraftData ToData() => new(Settings, [.. poolers], [.. picks], [.. pool]);

    public PoolEntry? EntryIn(Guid poolerId, int round) =>
        picks.Find(p => p.Fills(poolerId, round)) is { } pick ? Entry(pick.EntryId) : null;

    public IReadOnlyList<PoolEntry?> EntriesOf(Guid poolerId, int? exceptRound = null) =>
        [.. picks.Where(p => p.PoolerId == poolerId && p.Round != exceptRound).Select(p => Entry(p.EntryId))];

    public Tally TallyOf(Guid poolerId) => Tally.Of(EntriesOf(poolerId));

    public Change UpdateSettings(Settings settings)
    {
        if (settings.Problem() is { } problem)
            return Change.Invalid(problem);
        if (settings.MaxPoolers < poolers.Count)
            return Change.Invalid($"Maximum poolers cannot be below the {poolers.Count} poolers already added.");
        if (MidDraftSettings.Misfit(this, settings) is { } misfit)
            return Change.Invalid(misfit);
        Settings = settings;
        return Change.Ok;
    }

    public Change Pick(Guid poolerId, int round, string entryId)
    {
        var pooler = poolers.Find(p => p.Id == poolerId);
        if (pooler is null)
            return Change.NotFound;
        if (round < 1 || round > Settings.Rounds)
            return Change.Invalid($"Round must be between 1 and {Settings.Rounds}.");
        if (Entry(entryId) is not { } entry)
            return Change.EntryNotFound;
        if (PickRules.Check(this, pooler, round, entry) is { } refusal)
            return Change.Refused(refusal);
        picks.RemoveAll(p => p.Fills(poolerId, round));
        picks.Add(new Pick(poolerId, round, entryId));
        return Change.Ok;
    }

    public Change ClearPick(Guid poolerId, int round)
    {
        if (!poolers.Exists(p => p.Id == poolerId))
            return Change.NotFound;
        picks.RemoveAll(p => p.Fills(poolerId, round));
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

    private PoolEntry? Entry(string id) => pool.Find(e => e.Id == id);

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
