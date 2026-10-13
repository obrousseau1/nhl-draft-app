using NhlDraftApp.Api.Drafts;
using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Tests.Drafts;

public class PickRulesTests
{
    private const string Alex = "Alex";
    private const string Sam = "Sam";

    private static readonly Settings NoMinimums = new(Rounds: 20, Cap: 10m, DefenseMin: 0, Goalies: 2, Teams: 2);

    private static readonly PoolEntry McDavid = TestPool.Player("McDavid", Position.C, 12.5m);
    private static readonly PoolEntry Seven = TestPool.Player("Seven", Position.C, 7m);
    private static readonly PoolEntry Five = TestPool.Player("Five", Position.LW, 5m);
    private static readonly PoolEntry FiveAndHalf = TestPool.Player("FiveAndHalf", Position.RW, 5.5m);
    private static readonly PoolEntry Four = TestPool.Player("Four", Position.C, 4m);
    private static readonly PoolEntry Forward = TestPool.Player("Forward", Position.C, 1m);
    private static readonly PoolEntry OtherForward = TestPool.Player("OtherForward", Position.RW, 1m);
    private static readonly PoolEntry ThirdForward = TestPool.Player("ThirdForward", Position.LW, 1m);
    private static readonly PoolEntry Defense = TestPool.Player("Defense", Position.D, 1m);
    private static readonly PoolEntry OtherDefense = TestPool.Player("OtherDefense", Position.D, 1m);
    private static readonly PoolEntry ThirdDefense = TestPool.Player("ThirdDefense", Position.D, 1m);
    private static readonly PoolEntry Goalie = TestPool.Player("Goalie", Position.G, 1m);
    private static readonly PoolEntry OtherGoalie = TestPool.Player("OtherGoalie", Position.G, 1m);
    private static readonly PoolEntry ThirdGoalie = TestPool.Player("ThirdGoalie", Position.G, 1m);
    private static readonly PoolEntry Oilers = TestPool.Team("Oilers");
    private static readonly PoolEntry Flames = TestPool.Team("Flames");
    private static readonly PoolEntry Canucks = TestPool.Team("Canucks");

    private static readonly PoolEntry[] Pool =
    [
        McDavid, Seven, Five, FiveAndHalf, Four, Forward, OtherForward, ThirdForward,
        Defense, OtherDefense, ThirdDefense, Goalie, OtherGoalie, ThirdGoalie, Oilers, Flames, Canucks,
    ];

    [Fact]
    public void Pick_When_PlayerTakenByAnotherPooler_Should_RefuseNamingOwnerAndKeepState()
    {
        var draft = TestPool.Create(new Settings(), Pool, Alex, Sam);
        draft.PickInOrder(0, McDavid);
        var before = draft.ToData();

        var change = draft.Pick(draft.Poolers[1].Id, 1, McDavid.Id);

        change.Status.Should().Be(ChangeStatus.Refused);
        change.Code.Should().Be(PickRule.Taken);
        change.Reason.Should().Contain(Alex).And.Contain(McDavid.Name);
        draft.ToData().Should().BeEquivalentTo(before);
    }

    [Fact]
    public void Pick_When_CapHitAboveCapRemaining_Should_Refuse()
    {
        var draft = TestPool.Create(NoMinimums, Pool, Sam);
        draft.PickInOrder(0, Seven);

        var change = draft.Pick(draft.Poolers[0].Id, 2, Four.Id);

        change.Status.Should().Be(ChangeStatus.Refused);
        change.Code.Should().Be(PickRule.OverCap);
        change.Reason.Should().Contain("cap");
    }

    [Fact]
    public void Pick_When_ReplacedPlayerFreesEnoughCap_Should_Accept()
    {
        var draft = TestPool.Create(NoMinimums, Pool, Sam);
        draft.PickInOrder(0, Five, Four);

        draft.Pick(draft.Poolers[0].Id, 1, FiveAndHalf.Id).Should().Be(Change.Ok);

        draft.Picks.Should().HaveCount(2).And.Contain(new Pick(draft.Poolers[0].Id, 1, FiveAndHalf.Id));
    }

    [Fact]
    public void Pick_When_GoalieLimitReached_Should_Refuse()
    {
        var draft = TestPool.Create(NoMinimums, Pool, Sam);
        draft.PickInOrder(0, Goalie, OtherGoalie);

        var change = draft.Pick(draft.Poolers[0].Id, 3, ThirdGoalie.Id);

        change.Status.Should().Be(ChangeStatus.Refused);
        change.Code.Should().Be(PickRule.GoalieLimit);
        change.Reason.Should().Contain("goalie");
    }

    [Fact]
    public void Pick_When_TeamLimitReached_Should_Refuse()
    {
        var draft = TestPool.Create(NoMinimums, Pool, Sam);
        draft.PickInOrder(0, Oilers, Flames);

        var change = draft.Pick(draft.Poolers[0].Id, 3, Canucks.Id);

        change.Status.Should().Be(ChangeStatus.Refused);
        change.Code.Should().Be(PickRule.TeamLimit);
        change.Reason.Should().Contain("team");
    }

    [Fact]
    public void Pick_When_DefenseBeyondMinimumWithRoomLeft_Should_Accept()
    {
        var draft = TestPool.Create(new Settings(Rounds: 12), Pool, Sam);
        draft.PickInOrder(0, Defense, OtherDefense);

        draft.Pick(draft.Poolers[0].Id, 3, ThirdDefense.Id).Should().Be(Change.Ok);
    }

    [Fact]
    public void Pick_When_ForwardLeavesGoaliesUnreachable_Should_RefuseSayingGoaliesNeeded()
    {
        var draft = TestPool.Create(new Settings(Rounds: 4, DefenseMin: 0, Goalies: 2, Teams: 0), Pool, Sam);
        draft.PickInOrder(0, Forward, OtherForward);

        var change = draft.Pick(draft.Poolers[0].Id, 3, ThirdForward.Id);

        change.Status.Should().Be(ChangeStatus.Refused);
        change.Code.Should().Be(PickRule.Unreachable);
        change.Reason.Should().Contain("2 goalies");
    }

    [Theory]
    [InlineData(1, 0, 0, "1 defenseman")]
    [InlineData(0, 1, 0, "1 goalie")]
    [InlineData(0, 0, 1, "1 team")]
    public void Pick_When_ReplacingRequiredBoxWithForward_Should_RefuseWithWhatIsStillNeeded(int defenseMin, int goalies, int teams, string needed)
    {
        var required = defenseMin > 0 ? Defense : goalies > 0 ? Goalie : Oilers;
        var draft = TestPool.Create(new Settings(Rounds: 3, DefenseMin: defenseMin, Goalies: goalies, Teams: teams), Pool, Sam);
        draft.PickInOrder(0, Forward, required, OtherForward);

        var change = draft.Pick(draft.Poolers[0].Id, 2, ThirdForward.Id);

        change.Status.Should().Be(ChangeStatus.Refused);
        change.Code.Should().Be(PickRule.Unreachable);
        change.Reason.Should().Contain(needed);
    }

    [Fact]
    public void Pick_When_ReplacingDefenseBeyondMinimum_Should_Accept()
    {
        var draft = TestPool.Create(new Settings(Rounds: 3, DefenseMin: 1, Goalies: 0, Teams: 0), Pool, Sam);
        draft.PickInOrder(0, Defense, OtherDefense, Forward);

        draft.Pick(draft.Poolers[0].Id, 2, OtherForward.Id).Should().Be(Change.Ok);
    }

    [Fact]
    public void Pick_When_PoolerUnknown_Should_ReturnNotFound()
    {
        var draft = TestPool.Create(new Settings(), Pool, Sam);

        draft.Pick(Guid.NewGuid(), 1, McDavid.Id).Status.Should().Be(ChangeStatus.NotFound);
    }

    [Fact]
    public void Pick_When_EntryUnknown_Should_ReturnNotFound()
    {
        var draft = TestPool.Create(new Settings(), Pool, Sam);

        draft.Pick(draft.Poolers[0].Id, 1, "id-nobody").Status.Should().Be(ChangeStatus.NotFound);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    public void Pick_When_RoundOutOfRange_Should_RejectAsInvalid(int round)
    {
        var draft = TestPool.Create(new Settings(), Pool, Sam);

        draft.Pick(draft.Poolers[0].Id, round, McDavid.Id).Status.Should().Be(ChangeStatus.Invalid);
    }

    [Fact]
    public void Pick_When_ReplacingABox_Should_ReturnThePreviousEntryToThePool()
    {
        var draft = TestPool.Create(new Settings(), Pool, Alex, Sam);
        draft.PickInOrder(0, McDavid);

        draft.Pick(draft.Poolers[0].Id, 1, Seven.Id).Should().Be(Change.Ok);

        draft.Pick(draft.Poolers[1].Id, 1, McDavid.Id).Should().Be(Change.Ok);
    }

    [Fact]
    public void ClearPick_When_BoxFilled_Should_EmptyItAndReturnTheEntryToThePool()
    {
        var draft = TestPool.Create(new Settings(), Pool, Alex, Sam);
        draft.PickInOrder(0, McDavid);

        draft.ClearPick(draft.Poolers[0].Id, 1).Should().Be(Change.Ok);

        draft.Picks.Should().BeEmpty();
        draft.Pick(draft.Poolers[1].Id, 1, McDavid.Id).Should().Be(Change.Ok);
    }

    [Fact]
    public void ClearPick_When_PoolerUnknown_Should_ReturnNotFound()
    {
        var draft = TestPool.Create(new Settings(), Pool, Sam);

        draft.ClearPick(Guid.NewGuid(), 1).Status.Should().Be(ChangeStatus.NotFound);
    }
}
