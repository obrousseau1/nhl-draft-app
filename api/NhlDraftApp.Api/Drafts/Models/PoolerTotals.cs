namespace NhlDraftApp.Api.Drafts.Models;

public record PoolerTotals(decimal CapRemaining, decimal? AveragePerPick, int DefenseNeeded, int GoaliesNeeded, int TeamsNeeded)
{
    public static PoolerTotals Of(Draft draft, Pooler pooler)
    {
        var settings = draft.Settings;
        var tally = draft.TallyOf(pooler.Id);
        // WithinCap rounds, so a stray-decimal cap hit can overshoot by a fraction.
        var remaining = Math.Max(0m, settings.Cap - tally.CapUsed);
        // Teams cost no cap, so the cap left is spread over the player picks only.
        var playerBoxes = tally.EmptyBoxes(settings) - tally.TeamsNeeded(settings);
        decimal? average = playerBoxes > 0 ? Math.Round(remaining / playerBoxes, 3) : null;
        return new PoolerTotals(remaining, average, tally.DefenseNeeded(settings), tally.GoaliesNeeded(settings), tally.TeamsNeeded(settings));
    }
}
