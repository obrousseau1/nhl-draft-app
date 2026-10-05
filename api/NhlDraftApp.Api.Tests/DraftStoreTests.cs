using NhlDraftApp.Api.Drafts;

namespace NhlDraftApp.Api.Tests;

public sealed class DraftStoreTests : IDisposable
{
    private const string Alex = "Alex";
    private const string Sam = "Sam";
    private const string FileName = "draft.json";

    private static readonly Settings CustomSettings = new(Rounds: 16, Cap: 90.5m, DefenseMin: 3, Goalies: 2, Teams: 2);

    private readonly string folder = Path.Combine(Path.GetTempPath(), "nhl-draft-tests", Guid.NewGuid().ToString());
    private string FilePath => Path.Combine(folder, FileName);

    public void Dispose()
    {
        if (Directory.Exists(folder))
            Directory.Delete(folder, recursive: true);
    }

    [Fact]
    public void Constructor_When_FileMissing_Should_StartWithDefaults()
    {
        var store = new DraftStore(FilePath);

        store.Read(d => d.Settings).Should().Be(new Settings());
        store.Read(d => d.Poolers).Should().BeEmpty();
    }

    [Fact]
    public void Apply_When_ChangeSucceeds_Should_PersistForNextStore()
    {
        var store = new DraftStore(FilePath);
        store.Apply(d => d.UpdateSettings(CustomSettings));
        store.Apply(d => d.AddPooler(Alex));
        store.Apply(d => d.AddPooler(Sam));

        var reloaded = new DraftStore(FilePath);

        reloaded.Read(d => d.ToData()).Should().BeEquivalentTo(store.Read(d => d.ToData()));
        reloaded.Read(d => d.Settings).Should().Be(CustomSettings);
    }

    [Fact]
    public void Apply_When_Saved_Should_LeaveNoTempFile()
    {
        new DraftStore(FilePath).Apply(d => d.AddPooler(Alex));

        Directory.GetFiles(folder).Should().ContainSingle().Which.Should().Be(FilePath);
    }

    [Fact]
    public void Apply_When_ChangeRefused_Should_NotWriteFile()
    {
        var change = new DraftStore(FilePath).Apply(d => d.AddPooler(" "));

        change.Status.Should().Be(ChangeStatus.Invalid);
        File.Exists(FilePath).Should().BeFalse();
    }

    [Fact]
    public void DefaultPath_When_NotConfigured_Should_BeUnderLocalAppData()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        DraftStore.DefaultPath.Should().Be(Path.Combine(localAppData, "NhlDraftApp", FileName));
    }
}
