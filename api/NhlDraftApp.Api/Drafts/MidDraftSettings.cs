using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Drafts;

public static class MidDraftSettings
{
    public static SettingsMisfit? Misfit(Draft draft, Settings next)
    {
        if (!draft.Started)
            return null;
        var lastRound = draft.Picks.Max(p => p.Round);
        if (next.Rounds < lastRound)
            return new(MisfitRule.RoundsBelowPicks, $"Rounds cannot go below {lastRound}: round {lastRound} already has picks.");
        return draft.Poolers.Select(p => Misfit(draft.TallyOf(p.Id), p, next)).FirstOrDefault(m => m is not null);
    }

    private static SettingsMisfit? Misfit(Tally tally, Pooler pooler, Settings next)
    {
        if (!tally.WithinCap(next))
            return new(MisfitRule.OverCap, FormattableString.Invariant($"{pooler.Name} already uses {tally.CapUsed} M$, more than a {next.Cap} M$ cap."));
        if (tally.Goalies > next.Goalies)
            return new(MisfitRule.TooManyGoalies, $"{pooler.Name} already has {tally.Goalies} goalies, more than {next.Goalies}.");
        if (tally.Teams > next.Teams)
            return new(MisfitRule.TooManyTeams, $"{pooler.Name} already has {tally.Teams} teams, more than {next.Teams}.");
        return tally.Needed(next) > tally.EmptyBoxes(next)
            ? new(MisfitRule.Unreachable, $"{pooler.Name} would not have enough boxes left for the defensemen, goalies and teams still needed.")
            : null;
    }
}
