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
    private static readonly PoolEntry[] Pool = [.. Players, Goalie, OtherGoalie];

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
        change.Reason.Should().Contain(Sam).And.Contain("goalies");
        draft.Settings.Should().Be(Cap100);
    }

    [Fact]
    public void UpdateSettings_When_RoundsBelowLastFilledRound_Should_Refuse()
    {
        var draft = TestPool.Create(Cap100, Pool, Sam);
        draft.PickInOrder(0, Players[..5]);

        draft.UpdateSettings(Cap100 with { Rounds = 4 }).Status.Should().Be(ChangeStatus.Invalid);
    }

    [Fact]
    public void UpdateSettings_When_CapBelowCapUsed_Should_Refuse()
    {
        var draft = TestPool.Create(Cap100, Pool, Sam);
        draft.PickInOrder(0, Players[..5]);

        draft.UpdateSettings(Cap100 with { Cap = 29m }).Status.Should().Be(ChangeStatus.Invalid);
    }

    [Fact]
    public void UpdateSettings_When_NeededPicksNoLongerReachable_Should_Refuse()
    {
        var cap200 = Cap100 with { Cap = 200m };
        var draft = TestPool.Create(cap200, Pool, Sam);
        draft.PickInOrder(0, Players[..18]);

        draft.UpdateSettings(cap200 with { DefenseMin = 1 }).Status.Should().Be(ChangeStatus.Invalid);
    }
}
