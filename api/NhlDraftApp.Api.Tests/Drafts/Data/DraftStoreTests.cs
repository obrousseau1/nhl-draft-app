using NhlDraftApp.Api.Drafts;
using NhlDraftApp.Api.Drafts.Data;
using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Tests.Drafts.Data;

public class DraftStoreTests
{
    private const string Alex = "Alex";

    private static readonly Settings CustomSettings = new(Rounds: 16, Cap: 90.5m, DefenseMin: 3, Goalies: 2, Teams: 2);

    private readonly IDraftFile file = Substitute.For<IDraftFile>();

    [Fact]
    public void Constructor_When_FileEmpty_Should_StartWithDefaults()
    {
        file.Load().Returns((DraftData?)null);

        var store = new DraftStore(file);

        store.Read().Settings.Should().Be(new Settings());
        store.Read().Poolers.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_When_FileHasData_Should_LoadIt()
    {
        file.Load().Returns(new DraftData(CustomSettings, [new Pooler(Guid.NewGuid(), Alex)], []));

        var store = new DraftStore(file);

        store.Read().Settings.Should().Be(CustomSettings);
        store.Read().Poolers.Select(p => p.Name).Should().Equal(Alex);
    }

    [Fact]
    public void Apply_When_ChangeSucceeds_Should_SaveNewState()
    {
        var store = new DraftStore(file);

        store.Apply(d => d.AddPooler(Alex));

        file.Received(1).Save(Arg.Is<DraftData>(d => d.Poolers.Single().Name == Alex));
    }

    [Fact]
    public void Apply_When_ChangeSucceeds_Should_ReturnViewAfterChange()
    {
        var applied = new DraftStore(file).Apply(d => d.AddPooler(Alex));

        applied.Change.Should().Be(Change.Ok);
        applied.View.Poolers.Select(p => p.Name).Should().Equal(Alex);
    }

    [Fact]
    public void Apply_When_ChangeRefused_Should_NotSave()
    {
        var applied = new DraftStore(file).Apply(d => d.AddPooler(" "));

        applied.Change.Status.Should().Be(ChangeStatus.Invalid);
        file.DidNotReceive().Save(Arg.Any<DraftData>());
    }

    [Fact]
    public void Apply_When_SaveFails_Should_KeepMemoryUnchanged()
    {
        file.When(f => f.Save(Arg.Any<DraftData>())).Do(_ => throw new IOException());
        var store = new DraftStore(file);

        var apply = () => store.Apply(d => d.AddPooler(Alex));

        apply.Should().Throw<IOException>();
        store.Read().Poolers.Should().BeEmpty();
    }

    [Fact]
    public void StartNew_When_DraftStarted_Should_KeepSettingsAndPoolersAndClearPicks()
    {
        var alex = new Pooler(Guid.NewGuid(), Alex);
        file.Load().Returns(new DraftData(CustomSettings, [alex], [new Pick(alex.Id, 1, "entry-1")]));
        var store = new DraftStore(file);

        var view = store.StartNew();

        view.Settings.Should().Be(CustomSettings);
        view.Poolers.Should().Equal(alex);
        view.Started.Should().BeFalse();
        file.Received(1).SaveAsNew(Arg.Is<DraftData>(d => d.Picks.Count == 0 && d.Poolers.Single() == alex));
    }

    [Fact]
    public void StartNew_When_SaveFails_Should_KeepCurrentDraft()
    {
        var alex = new Pooler(Guid.NewGuid(), Alex);
        file.Load().Returns(new DraftData(CustomSettings, [alex], [new Pick(alex.Id, 1, "entry-1")]));
        file.When(f => f.SaveAsNew(Arg.Any<DraftData>())).Do(_ => throw new IOException());
        var store = new DraftStore(file);

        var startNew = () => store.StartNew();

        startNew.Should().Throw<IOException>();
        store.Read().Started.Should().BeTrue();
    }
}
