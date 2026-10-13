using NhlDraftApp.Api.Drafts;
using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Tests.Drafts;

public class PoolerTotalsTests
{
    private const string Alex = "Alex";

    private static readonly PoolEntry McDavid = TestPool.Player("McDavid", Position.C, 12.5m);
    private static readonly PoolEntry Defense = TestPool.Player("Defense", Position.D, 3.5m);
    private static readonly PoolEntry Forward = TestPool.Player("Forward", Position.LW, 1m);
    private static readonly PoolEntry Goalie = TestPool.Player("Goalie", Position.G, 1m);
    private static readonly PoolEntry OtherGoalie = TestPool.Player("OtherGoalie", Position.G, 1m);
    private static readonly PoolEntry StrayDecimals = TestPool.Player("StrayDecimals", Position.C, 100.0004m);
    private static readonly PoolEntry[] Pool = [McDavid, Defense, Forward, Goalie, OtherGoalie, StrayDecimals];

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

    [Theory]
    [InlineData(0, 2.5)]
    [InlineData(1, 3.333)]
    [InlineData(2, 5.0)]
    [InlineData(3, 10.0)]
    [InlineData(4, null)]
    public void Of_When_TeamsRequiredVary_Should_KeepTheirBoxesOutOfTheAverage(int teams, double? average)
    {
        var draft = TestPool.Create(new Settings(Rounds: 4, Cap: 10m, DefenseMin: 0, Goalies: 0, Teams: teams), Pool, Alex);

        var totals = PoolerTotals.Of(draft, draft.Poolers[0]);

        totals.AveragePerPick.Should().Be((decimal?)average);
        totals.TeamsNeeded.Should().Be(teams);
    }

    [Theory]
    [InlineData(1, 1, 6.059)]
    [InlineData(2, 0, 6.375)]
    public void Of_When_GoaliesPicked_Should_CountThemAndSpreadTheCapLeft(int picked, int needed, double average)
    {
        var draft = TestPool.Create(new Settings(), Pool, Alex);
        draft.PickInOrder(0, [.. new[] { Goalie, OtherGoalie }.Take(picked)]);

        var totals = PoolerTotals.Of(draft, draft.Poolers[0]);

        totals.Should().Be(new PoolerTotals(CapRemaining: 104m - picked, AveragePerPick: (decimal)average, DefenseNeeded: 2, GoaliesNeeded: needed, TeamsNeeded: 2));
    }

    [Fact]
    public void Of_When_GoalieAndDefensemanPicked_Should_LowerBothNeeds()
    {
        var draft = TestPool.Create(new Settings(), Pool, Alex);
        draft.PickInOrder(0, Goalie, Defense);

        var totals = PoolerTotals.Of(draft, draft.Poolers[0]);

        totals.Should().Be(new PoolerTotals(CapRemaining: 99.5m, AveragePerPick: 6.219m, DefenseNeeded: 1, GoaliesNeeded: 1, TeamsNeeded: 2));
    }

    [Fact]
    public void Of_When_StrayDecimalsOvershootTheCap_Should_NeverGoBelowZero()
    {
        var draft = TestPool.Create(new Settings(Rounds: 1, Cap: 100m, DefenseMin: 0, Goalies: 0, Teams: 0), Pool, Alex);
        draft.PickInOrder(0, StrayDecimals);

        PoolerTotals.Of(draft, draft.Poolers[0]).CapRemaining.Should().Be(0m);
    }
}
