using Microsoft.Extensions.Logging.Abstractions;
using NhlDraftApp.Api.Drafts;
using NhlDraftApp.Api.Drafts.Data;
using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Tests.Drafts.Data;

public sealed class DraftFileTests : IDisposable
{
    private const string FirstName = "draft-20261005-221930-000.json";
    private const string SecondName = "draft-20261006-090000-500.json";

    private static readonly DateTimeOffset FirstTime = new(2026, 10, 5, 22, 19, 30, TimeSpan.Zero);
    private static readonly DateTimeOffset SecondTime = new(2026, 10, 6, 9, 0, 0, 500, TimeSpan.Zero);
    private static readonly Settings CustomSettings = new(Rounds: 16, Cap: 90.5m, DefenseMin: 3, Goalies: 2, Teams: 2);
    private static readonly DraftData Data = new(CustomSettings, [new Pooler(Guid.NewGuid(), "Alex")], []);
    private static readonly DraftData OtherData = new(new Settings(), [new Pooler(Guid.NewGuid(), "Sam")], []);

    private readonly string folder = Path.Combine(Path.GetTempPath(), "nhl-draft-tests", Guid.NewGuid().ToString());
    private readonly TimeProvider time = Substitute.For<TimeProvider>();

    public DraftFileTests()
    {
        time.LocalTimeZone.Returns(TimeZoneInfo.Utc);
        time.GetUtcNow().Returns(FirstTime, SecondTime);
    }

    public void Dispose()
    {
        if (Directory.Exists(folder))
            Directory.Delete(folder, recursive: true);
    }

    [Fact]
    public void Load_When_FolderEmpty_Should_ReturnNull()
    {
        NewFile().Load().Should().BeNull();
    }

    [Fact]
    public void Save_When_NoDraftYet_Should_CreateTimestampedFile()
    {
        NewFile().Save(Data);

        DraftNames().Should().Equal(FirstName);
    }

    [Fact]
    public void Save_When_LoadedBack_Should_ReturnSameData()
    {
        NewFile().Save(Data);

        NewFile().Load().Should().BeEquivalentTo(Data);
    }

    [Fact]
    public void Load_When_SeveralDrafts_Should_LoadNewest()
    {
        Directory.CreateDirectory(folder);
        var file = NewFile();
        file.Save(Data);
        file.SaveAsNew(OtherData);

        NewFile().Load().Should().BeEquivalentTo(OtherData);
    }

    [Fact]
    public void SaveAsNew_When_DraftExists_Should_KeepPreviousFile()
    {
        var file = NewFile();
        file.Save(Data);

        file.SaveAsNew(OtherData);
        file.Save(OtherData with { Settings = CustomSettings });

        DraftNames().Should().Equal(FirstName, SecondName);
        File.ReadAllText(Path.Combine(folder, FirstName)).Should().Contain("Alex").And.NotContain("Sam");
    }

    [Fact]
    public void Save_When_Done_Should_LeaveNoTempFile()
    {
        NewFile().Save(Data);

        Directory.GetFiles(folder).Should().ContainSingle();
    }

    [Theory]
    [InlineData("{ not json")]
    [InlineData("{\"settings\":{}}")]
    public void Load_When_FileCorrupt_Should_ReturnNullAndKeepCorruptCopy(string content)
    {
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, FirstName), content);
        var file = NewFile();

        file.Load().Should().BeNull();
        file.Save(Data);

        Directory.GetFiles(folder, FirstName + ".corrupt-*").Should().ContainSingle()
            .Which.Should().Match(p => File.ReadAllText(p) == content);
        DraftNames().Should().Equal(SecondName);
    }

    [Fact]
    public void DefaultFolder_When_NotConfigured_Should_BeUnderLocalAppData()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        DraftFile.DefaultFolder.Should().Be(Path.Combine(localAppData, "NhlDraftApp"));
    }

    private DraftFile NewFile() => new(folder, NullLogger<DraftFile>.Instance, time);

    private IEnumerable<string> DraftNames() =>
        Directory.GetFiles(folder, "draft-*.json").Select(Path.GetFileName).Order()!;
}
