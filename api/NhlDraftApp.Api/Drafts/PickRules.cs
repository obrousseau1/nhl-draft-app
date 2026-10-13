using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Drafts;

public static class PickRules
{
    // First failing rule wins. The box's current pick is left out, so a replacement is judged as if the box were empty.
    public static PickRefusal? Check(Draft draft, Pooler pooler, int round, PoolEntry entry)
    {
        var others = draft.EntriesOf(pooler.Id, round);
        var before = Tally.Of(others);
        var after = Tally.Of([.. others, entry]);
        return Owner(draft, pooler, round, entry)
            ?? Cap(draft.Settings, pooler, entry, before, after)
            ?? Limit(draft.Settings, pooler, entry, after)
            ?? Reachable(draft.Settings, pooler, after);
    }

    private static PickRefusal? Owner(Draft draft, Pooler pooler, int round, PoolEntry entry)
    {
        var taken = draft.Picks.FirstOrDefault(p => p.EntryId == entry.Id && !p.Fills(pooler.Id, round));
        if (taken is null)
            return null;
        var owner = draft.Poolers.First(p => p.Id == taken.PoolerId).Name;
        return new(PickRule.Taken, $"{entry.Name} is already picked by {owner} (round {taken.Round}).");
    }

    private static PickRefusal? Cap(Settings settings, Pooler pooler, PoolEntry entry, Tally before, Tally after) =>
        after.WithinCap(settings)
            ? null
            : new(PickRule.OverCap, FormattableString.Invariant($"Over the cap: {entry.Name} costs {entry.CapHit} M$ and {pooler.Name} has {settings.Cap - before.CapUsed} M$ left."));

    private static PickRefusal? Limit(Settings settings, Pooler pooler, PoolEntry entry, Tally after) => entry.Position switch
    {
        Position.G when after.Goalies > settings.Goalies => new(PickRule.GoalieLimit, $"{pooler.Name} has reached the goalie limit of {settings.Goalies}."),
        Position.Team when after.Teams > settings.Teams => new(PickRule.TeamLimit, $"{pooler.Name} has reached the team limit of {settings.Teams}."),
        _ => null,
    };

    private static PickRefusal? Reachable(Settings settings, Pooler pooler, Tally after)
    {
        var empty = after.EmptyBoxes(settings);
        return after.Needed(settings) <= empty
            ? null
            : new(PickRule.Unreachable, $"Not enough boxes left: {pooler.Name} still needs {Needs(after, settings)}, with {Count(empty, "empty box", "empty boxes")} after this pick.");
    }

    private static string Needs(Tally tally, Settings settings)
    {
        (int Count, string One, string Many)[] needs =
        [
            (tally.DefenseNeeded(settings), "defenseman", "defensemen"),
            (tally.GoaliesNeeded(settings), "goalie", "goalies"),
            (tally.TeamsNeeded(settings), "team", "teams"),
        ];
        return string.Join(", ", needs.Where(n => n.Count > 0).Select(n => Count(n.Count, n.One, n.Many)));
    }

    private static string Count(int count, string one, string many) => $"{count} {(count == 1 ? one : many)}";
}
