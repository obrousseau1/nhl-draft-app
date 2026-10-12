using NhlDraftApp.Api.Drafts;
using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Tests.Drafts;

public class SnakeTests
{
    private static readonly Settings FourRounds = new(Rounds: 4, DefenseMin: 0, Goalies: 0, Teams: 0);
    private static readonly PoolEntry[] Pool = [.. Enumerable.Range(1, 16).Select(i => TestPool.Player($"P{i}", Position.C, 1m))];

    [Fact]
    public void Current_When_RoundOneComplete_Should_ReverseOrderInRoundTwo()
    {
        var draft = TestPool.Create(FourRounds, Pool, "A", "B", "C");
        PickBox(draft, 0, 1, 0);
        PickBox(draft, 1, 1, 1);
        PickBox(draft, 2, 1, 2);

        Snake.Current(draft).Should().Be(new Box(draft.Poolers[2].Id, 2));
    }

    [Fact]
    public void Current_When_EarlierBoxSkipped_Should_StayOnTheSkippedBox()
    {
        var draft = TestPool.Create(FourRounds, Pool, "A", "B", "C");
        PickBox(draft, 0, 1, 0);
        PickBox(draft, 2, 1, 1);

        Snake.Current(draft).Should().Be(new Box(draft.Poolers[1].Id, 1));
    }

    [Fact]
    public void Current_When_PickMadeOutOfTurn_Should_SaveItWithoutMoving()
    {
        var draft = TestPool.Create(FourRounds, Pool, "A", "B", "C", "D");
        var entry = 0;
        for (var round = 1; round <= 2; round++)
        {
            for (var pooler = 0; pooler < 4; pooler++)
                PickBox(draft, pooler, round, entry++);
        }

        PickBox(draft, 3, 3, entry);

        draft.Picks.Should().Contain(new Pick(draft.Poolers[3].Id, 3, Pool[entry].Id));
        Snake.Current(draft).Should().Be(new Box(draft.Poolers[0].Id, 3));
    }

    [Fact]
    public void Current_When_NoPoolers_Should_BeNull()
    {
        Snake.Current(TestPool.Create(FourRounds, Pool)).Should().BeNull();
    }

    [Fact]
    public void Current_When_EveryBoxFilled_Should_BeNull()
    {
        var draft = TestPool.Create(new Settings(Rounds: 1, DefenseMin: 0, Goalies: 0, Teams: 0), Pool, "A");
        PickBox(draft, 0, 1, 0);

        Snake.Current(draft).Should().BeNull();
    }

    private static void PickBox(Draft draft, int pooler, int round, int entry) =>
        draft.Pick(draft.Poolers[pooler].Id, round, Pool[entry].Id).Should().Be(Change.Ok);
}
