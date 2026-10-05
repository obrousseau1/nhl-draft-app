using NhlDraftApp.Api.Drafts;

namespace NhlDraftApp.Api.Tests;

public class DraftTests
{
    private const string Alex = "Alex";
    private const string Sam = "Sam";
    private const string Max = "Max";
    private const string Alexandre = "Alexandre";
    private const string EntryId = "entry-1";

    private static readonly Settings DefaultSettings = new();
    private static readonly Settings CustomSettings = new(Rounds: 16, Cap: 90m, DefenseMin: 3, Goalies: 2, Teams: 2);

    [Fact]
    public void Settings_When_DraftIsNew_Should_UseDefaults()
    {
        new Draft().Settings.Should().Be(new Settings(Rounds: 20, Cap: 104m, DefenseMin: 2, Goalies: 2, Teams: 2));
    }

    [Theory]
    [InlineData(5, 104, 2, 2, 2)]
    [InlineData(0, 104, 0, 0, 0)]
    [InlineData(20, 0, 2, 2, 2)]
    [InlineData(20, 104, -1, 2, 2)]
    [InlineData(20, -5, 2, 2, 2)]
    public void UpdateSettings_When_Inconsistent_Should_RejectAsInvalid(int rounds, int cap, int defenseMin, int goalies, int teams)
    {
        var draft = new Draft();

        var change = draft.UpdateSettings(new Settings(rounds, cap, defenseMin, goalies, teams));

        change.Status.Should().Be(ChangeStatus.Invalid);
        change.Reason.Should().NotBeNullOrWhiteSpace();
        draft.Settings.Should().Be(DefaultSettings);
    }

    [Fact]
    public void UpdateSettings_When_Valid_Should_Apply()
    {
        var draft = new Draft();

        draft.UpdateSettings(CustomSettings).Should().Be(Change.Ok);

        draft.Settings.Should().Be(CustomSettings);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("alex")]
    [InlineData(" Alex ")]
    public void AddPooler_When_NameEmptyOrDuplicate_Should_RejectAsInvalid(string name)
    {
        var draft = WithPoolers(Alex);

        draft.AddPooler(name).Status.Should().Be(ChangeStatus.Invalid);

        draft.Poolers.Should().ContainSingle();
    }

    [Fact]
    public void AddPooler_When_NameValid_Should_AppendTrimmed()
    {
        var draft = WithPoolers(Alex);

        draft.AddPooler($"  {Sam} ").Should().Be(Change.Ok);

        Names(draft).Should().Equal(Alex, Sam);
    }

    [Fact]
    public void RenamePooler_When_NameTakenByAnother_Should_RejectAsInvalid()
    {
        var draft = WithPoolers(Alex, Sam);

        draft.RenamePooler(draft.Poolers[1].Id, Alex.ToUpperInvariant()).Status.Should().Be(ChangeStatus.Invalid);
    }

    [Fact]
    public void RenamePooler_When_SameNameDifferentCase_Should_Apply()
    {
        var draft = WithPoolers(Alex);

        draft.RenamePooler(draft.Poolers[0].Id, Alex.ToUpperInvariant()).Should().Be(Change.Ok);

        draft.Poolers[0].Name.Should().Be(Alex.ToUpperInvariant());
    }

    [Fact]
    public void RenamePooler_When_UnknownId_Should_ReturnNotFound()
    {
        WithPoolers(Alex).RenamePooler(Guid.NewGuid(), Sam).Status.Should().Be(ChangeStatus.NotFound);
    }

    [Fact]
    public void RemovePooler_When_NotStarted_Should_Remove()
    {
        var draft = WithPoolers(Alex, Sam);

        draft.RemovePooler(draft.Poolers[0].Id).Should().Be(Change.Ok);

        Names(draft).Should().Equal(Sam);
    }

    [Fact]
    public void Reorder_When_SameSetOfIds_Should_ApplyNewOrder()
    {
        var draft = WithPoolers(Alex, Sam, Max);

        draft.Reorder(ReversedIds(draft)).Should().Be(Change.Ok);

        Names(draft).Should().Equal(Max, Sam, Alex);
    }

    [Fact]
    public void Reorder_When_IdsDoNotMatchPoolers_Should_RejectAsInvalid()
    {
        var draft = WithPoolers(Alex, Sam);
        var first = draft.Poolers[0].Id;

        draft.Reorder([first, Guid.NewGuid()]).Status.Should().Be(ChangeStatus.Invalid);
        draft.Reorder([first, first]).Status.Should().Be(ChangeStatus.Invalid);
    }

    [Fact]
    public void Shuffle_When_RandomSwaps_Should_KeepSamePoolersInNewOrder()
    {
        var draft = WithPoolers(Alex, Sam, Max);
        var random = Substitute.For<Random>();
        random.Next(Arg.Any<int>(), Arg.Any<int>()).Returns(call => call.ArgAt<int>(1) - 1);

        draft.Shuffle(random).Should().Be(Change.Ok);

        Names(draft).Should().BeEquivalentTo(Alex, Sam, Max).And.NotEqual([Alex, Sam, Max]);
    }

    [Fact]
    public void SetupChanges_When_DraftStarted_Should_BeLocked()
    {
        var draft = Started(Alex, Sam);

        draft.UpdateSettings(CustomSettings).Status.Should().Be(ChangeStatus.Locked);
        draft.AddPooler(Max).Status.Should().Be(ChangeStatus.Locked);
        draft.RemovePooler(draft.Poolers[1].Id).Status.Should().Be(ChangeStatus.Locked);
        draft.Reorder(ReversedIds(draft)).Status.Should().Be(ChangeStatus.Locked);
        draft.Shuffle(new Random(1)).Status.Should().Be(ChangeStatus.Locked);

        draft.Settings.Should().Be(DefaultSettings);
        Names(draft).Should().Equal(Alex, Sam);
    }

    [Fact]
    public void RenamePooler_When_DraftStarted_Should_ApplyAndKeepPicks()
    {
        var draft = Started(Alex);

        draft.RenamePooler(draft.Poolers[0].Id, Alexandre).Should().Be(Change.Ok);

        draft.Poolers[0].Name.Should().Be(Alexandre);
        draft.Picks.Should().ContainSingle();
    }

    [Fact]
    public void Reset_When_DraftStarted_Should_ClearPicksOnlyAndUnlockSetup()
    {
        var draft = Started(Alex, Sam);

        draft.Reset().Should().Be(Change.Ok);

        draft.Picks.Should().BeEmpty();
        draft.Poolers.Should().HaveCount(2);
        draft.AddPooler(Max).Should().Be(Change.Ok);
    }

    private static Draft WithPoolers(params string[] names)
    {
        var draft = new Draft();
        foreach (var name in names)
            draft.AddPooler(name);

        return draft;
    }

    private static Draft Started(params string[] names)
    {
        var poolers = names.Select(n => new Pooler(Guid.NewGuid(), n)).ToList();
        return new Draft(new DraftData(DefaultSettings, poolers, [new Pick(poolers[0].Id, 1, EntryId)]));
    }

    private static IEnumerable<string> Names(Draft draft) => draft.Poolers.Select(p => p.Name);

    private static List<Guid> ReversedIds(Draft draft) => draft.Poolers.Select(p => p.Id).Reverse().ToList();
}
