using NhlDraftApp.Api.Drafts;
using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Tests.Drafts;

public class PoolerTotalsTests
{
    private const string Alex = "Alex";

    private static readonly PoolEntry McDavid = TestPool.Player("McDavid", Position.C, 12.5m);
    private static readonly PoolEntry Defense = TestPool.Player("Defense", Position.D, 3.5m);
    private static readonly PoolEntry Forward = TestPool.Player("Forward", Position.LW, 1m);
    private static readonly PoolEntry[] Pool = [McDavid, Defense, Forward];

    [Fact]
    public void Of_When_McDavidAndADefensemanPicked_Should_MatchTheSpecExample()
    {
        var draft = TestPool.Create(new Settings(), Pool, Alex);
        draft.PickInOrder(0, McDavid, Defense);

        var totals = PoolerTotals.Of(draft, draft.Poolers[0]);

        totals.Should().Be(new PoolerTotals(CapRemaining: 88.0m, AveragePerPick: 5.5m, DefenseNeeded: 1, GoaliesNeeded: 2, TeamsNeeded: 2));
    }

    [Fact]
    public void Of_When_OnlyTeamPicksRemain_Should_LeaveTheAverageBlank()
    {
        var draft = TestPool.Create(new Settings(Rounds: 3, DefenseMin: 0, Goalies: 0, Teams: 2), Pool, Alex);
        draft.PickInOrder(0, Forward);

        PoolerTotals.Of(draft, draft.Poolers[0]).AveragePerPick.Should().BeNull();
    }
}
