using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using NhlDraftApp.Api.Drafts;

namespace NhlDraftApp.Api.Tests;

public sealed class DraftEndpointsTests : IDisposable
{
    private const string Alex = "Alex";
    private const string Sam = "Sam";
    private const string Max = "Max";
    private const string Alexandre = "Alexandre";
    private const string DraftRoute = "/api/draft";
    private const string SettingsRoute = "/api/draft/settings";
    private const string ResetRoute = "/api/draft/reset";
    private const string PoolersRoute = "/api/poolers";
    private const string OrderRoute = "/api/poolers/order";
    private const string ShuffleRoute = "/api/poolers/shuffle";

    private static readonly Settings CustomSettings = new(Rounds: 16, Cap: 90m, DefenseMin: 3, Goalies: 2, Teams: 2);

    private readonly string folder = Path.Combine(Path.GetTempPath(), "nhl-draft-tests", Guid.NewGuid().ToString());
    private readonly WebApplicationFactory<Program> factory;
    private HttpClient? client;
    private string FilePath => Path.Combine(folder, "draft.json");

    private HttpClient Client => client ??= factory.CreateClient();

    public DraftEndpointsTests()
    {
        factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b => b.UseSetting("Draft:DataPath", FilePath));
    }

    public void Dispose()
    {
        client?.Dispose();
        factory.Dispose();
        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, recursive: true);
        }
    }

    private record PoolerBody(Guid Id, string Name);

    private record DraftBody(Settings Settings, List<PoolerBody> Poolers, List<Pick> Picks, bool Started);

    private record ReasonBody(string Reason);

    [Fact]
    public async Task GetDraft_When_New_Should_ReturnDefaultsNotStarted()
    {
        DraftBody draft = await GetDraft();

        draft.Settings.Should().Be(new Settings());
        draft.Poolers.Should().BeEmpty();
        draft.Started.Should().BeFalse();
    }

    [Fact]
    public async Task PostPooler_When_Valid_Should_ReturnCreatedAndSaveToConfiguredPath()
    {
        HttpResponseMessage response = await AddPooler(Alex);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        File.ReadAllText(FilePath).Should().Contain(Alex);
    }

    [Fact]
    public async Task PostPooler_When_Duplicate_Should_ReturnBadRequestWithReason()
    {
        await AddPooler(Alex);

        HttpResponseMessage response = await AddPooler(Alex.ToLowerInvariant());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Reason(response)).Should().Contain("already exists");
    }

    [Fact]
    public async Task PutSettings_When_Valid_Should_ReturnUpdatedDraft()
    {
        HttpResponseMessage response = await Client.PutAsJsonAsync(SettingsRoute, CustomSettings);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<DraftBody>())!.Settings.Should().Be(CustomSettings);
    }

    [Fact]
    public async Task PutSettings_When_DraftStarted_Should_ReturnConflictWithReason()
    {
        SeedStartedDraft();

        HttpResponseMessage response = await Client.PutAsJsonAsync(SettingsRoute, CustomSettings);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await Reason(response)).Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task PutPooler_When_DraftStarted_Should_Rename()
    {
        SeedStartedDraft();
        Guid id = (await GetDraft()).Poolers[0].Id;

        HttpResponseMessage response = await Client.PutAsJsonAsync(PoolerRoute(id), new { name = Alexandre });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await PoolerNames()).Should().Equal(Alexandre);
    }

    [Fact]
    public async Task PutPooler_When_UnknownId_Should_ReturnNotFound()
    {
        HttpResponseMessage response = await Client.PutAsJsonAsync(PoolerRoute(Guid.NewGuid()), new { name = Sam });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task OrderShuffleDelete_When_NotStarted_Should_Apply()
    {
        foreach (string? name in new[] { Alex, Sam, Max })
        {
            await AddPooler(name);
        }

        List<Guid> ids = (await GetDraft()).Poolers.Select(p => p.Id).Reverse().ToList();

        (await Client.PutAsJsonAsync(OrderRoute, ids)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await PoolerNames()).Should().Equal(Max, Sam, Alex);

        (await Client.PostAsync(ShuffleRoute, null)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await Client.DeleteAsync(PoolerRoute(ids[0]))).StatusCode.Should().Be(HttpStatusCode.OK);
        (await PoolerNames()).Should().BeEquivalentTo(Sam, Alex);
    }

    [Fact]
    public async Task PostReset_When_DraftStarted_Should_ClearPicksAndUnlock()
    {
        SeedStartedDraft();

        (await Client.PostAsync(ResetRoute, null)).StatusCode.Should().Be(HttpStatusCode.OK);

        DraftBody draft = await GetDraft();
        draft.Started.Should().BeFalse();
        draft.Poolers.Should().ContainSingle();
    }

    private static string PoolerRoute(Guid id) => $"{PoolersRoute}/{id}";

    private async Task<DraftBody> GetDraft() => (await Client.GetFromJsonAsync<DraftBody>(DraftRoute))!;

    private async Task<IEnumerable<string>> PoolerNames() => (await GetDraft()).Poolers.Select(p => p.Name);

    private Task<HttpResponseMessage> AddPooler(string name) => Client.PostAsJsonAsync(PoolersRoute, new { name });

    private static async Task<string> Reason(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<ReasonBody>())!.Reason;

    private void SeedStartedDraft()
    {
        Pooler alex = new(Guid.NewGuid(), Alex);
        Directory.CreateDirectory(folder);
        DraftData data = new(new Settings(), [alex], [new Pick(alex.Id, 1, "entry-1")]);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(data, JsonSerializerOptions.Web));
    }
}
