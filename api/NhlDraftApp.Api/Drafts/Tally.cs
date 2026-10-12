using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Drafts;

public record Tally(int Picks, decimal CapUsed, int Defense, int Goalies, int Teams)
{
    // A null entry is a pick whose entry is missing from the pool: it fills a box but counts for nothing else.
    public static Tally Of(IReadOnlyCollection<PoolEntry?> entries) => new(
        entries.Count,
        entries.Sum(e => e?.CapHit ?? 0m),
        entries.Count(e => e?.Position == Position.D),
        entries.Count(e => e?.Position == Position.G),
        entries.Count(e => e?.Position == Position.Team));

    // Rounded to absorb kit cap hits with stray decimals, like 12.604.
    public bool WithinCap(Settings settings) => Math.Round(CapUsed, 3) <= settings.Cap;

    public int DefenseNeeded(Settings settings) => Math.Max(0, settings.DefenseMin - Defense);

    public int GoaliesNeeded(Settings settings) => Math.Max(0, settings.Goalies - Goalies);

    public int TeamsNeeded(Settings settings) => Math.Max(0, settings.Teams - Teams);

    public int Needed(Settings settings) => DefenseNeeded(settings) + GoaliesNeeded(settings) + TeamsNeeded(settings);

    public int EmptyBoxes(Settings settings) => settings.Rounds - Picks;
}
