using NhlDraftApp.Api.Drafts;
using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Tests.Drafts;

public class MidDraftSettingsTests
{
    private const string Alex = "Alex";
    private const string Sam = "Sam";

    private static readonly Settings Cap100 = new(Rounds: 20, Cap: 100m, DefenseMin: 0, Goalies: 2, Teams: 0);
    private static readonly PoolEntry[] Players = [.. Enumerable.Range(1, 20).Select(i => TestPool.Player($"P{i}", Position.C, 6m))];
    private static readonly PoolEntry Goalie = TestPool.Player("Goalie", Position.G, 1m);
    private static readonly PoolEntry OtherGoalie = TestPool.Player("OtherGoalie", Position.G, 1m);
    private static readonly PoolEntry Oilers = TestPool.Team("Oilers");
    private static readonly PoolEntry Flames = TestPool.Team("Flames");
    private static readonly PoolEntry[] Pool = [.. Players, Goalie, OtherGoalie, Oilers, Flames];

    [Fact]
    public void UpdateSettings_When_TenRoundsDraftedAndCapRaised_Should_ApplyAndKeepPicks()
    {
        var draft = TestPool.Create(Cap100, Pool, Alex, Sam);
        draft.PickInOrder(0, Players[..10]);
        draft.PickInOrder(1, Players[10..]);

        draft.UpdateSettings(Cap100 with { Cap = 104m }).Should().Be(Change.Ok);

        draft.Settings.Cap.Should().Be(104m);
        draft.Picks.Should().HaveCount(20);
    }

    [Fact]
    public void UpdateSettings_When_PoolerHasMoreGoaliesThanNewCount_Should_RefuseWithReason()
    {
        var draft = TestPool.Create(Cap100, Pool, Sam);
        draft.PickInOrder(0, Goalie, OtherGoalie);

        var change = draft.UpdateSettings(Cap100 with { Goalies = 1 });

        change.Status.Should().Be(ChangeStatus.Invalid);
        change.Code.Should().Be(MisfitRule.TooManyGoalies);
        change.Reason.Should().Contain(Sam).And.Contain("goalies");
        draft.Settings.Should().Be(Cap100);
    }

    [Fact]
    public void UpdateSettings_When_PoolerHasMoreTeamsThanNewCount_Should_RefuseWithReason()
    {
        var twoTeams = Cap100 with { Teams = 2 };
        var draft = TestPool.Create(twoTeams, Pool, Sam);
        draft.PickInOrder(0, Oilers, Flames);

        var change = draft.UpdateSettings(twoTeams with { Teams = 1 });

        change.Status.Should().Be(ChangeStatus.Invalid);
        change.Code.Should().Be(MisfitRule.TooManyTeams);
        change.Reason.Should().Contain(Sam).And.Contain("2 teams");
    }

    [Fact]
    public void UpdateSettings_When_RoundsBelowLastFilledRound_Should_RefuseWithReason()
    {
        var draft = TestPool.Create(Cap100, Pool, Sam);
        draft.PickInOrder(0, Players[..5]);

        var change = draft.UpdateSettings(Cap100 with { Rounds = 4 });

        change.Status.Should().Be(ChangeStatus.Invalid);
        change.Code.Should().Be(MisfitRule.RoundsBelowPicks);
        change.Reason.Should().Contain("round 5");
    }

    [Fact]
    public void UpdateSettings_When_CapBelowCapUsed_Should_RefuseWithReason()
    {
        var draft = TestPool.Create(Cap100, Pool, Sam);
        draft.PickInOrder(0, Players[..5]);

        var change = draft.UpdateSettings(Cap100 with { Cap = 29m });

        change.Status.Should().Be(ChangeStatus.Invalid);
        change.Code.Should().Be(MisfitRule.OverCap);
        change.Reason.Should().Contain(Sam).And.Contain("30 M$");
    }

    [Fact]
    public void UpdateSettings_When_NeededPicksNoLongerReachable_Should_RefuseWithReason()
    {
        var cap200 = Cap100 with { Cap = 200m };
        var draft = TestPool.Create(cap200, Pool, Sam);
        draft.PickInOrder(0, Players[..18]);

        var change = draft.UpdateSettings(cap200 with { DefenseMin = 1 });

        change.Status.Should().Be(ChangeStatus.Invalid);
        change.Code.Should().Be(MisfitRule.Unreachable);
        change.Reason.Should().Contain(Sam).And.Contain("not have enough boxes");
    }
}
