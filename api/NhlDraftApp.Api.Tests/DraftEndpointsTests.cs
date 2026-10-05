using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using NhlDraftApp.Api.Drafts;
using NhlDraftApp.Api.Drafts.Models;

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
    private const string NewDraftRoute = "/api/draft/new";

    private static readonly Settings CustomSettings = new(Rounds: 16, Cap: 90m, DefenseMin: 3, Goalies: 2, Teams: 2);

    private readonly string folder = Path.Combine(Path.GetTempPath(), "nhl-draft-tests", Guid.NewGuid().ToString());
    private readonly WebApplicationFactory<Program> factory;
    private HttpClient? client;
    private HttpClient Client => client ??= factory.CreateClient();

    public DraftEndpointsTests()
    {
        factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b => b.UseSetting("Draft:DataFolder", folder));
    }

    public void Dispose()
    {
        client?.Dispose();
        factory.Dispose();
        if (Directory.Exists(folder))
            Directory.Delete(folder, recursive: true);
    }

    private record PoolerBody(Guid Id, string Name);

    private record DraftBody(Settings Settings, List<PoolerBody> Poolers, List<Pick> Picks, bool Started);

    private record ProblemBody(int Status, string Detail);

    [Fact]
    public async Task GetDraft_When_New_Should_ReturnDefaultsNotStarted()
    {
        var draft = await GetDraft();

        draft.Settings.Should().Be(new Settings());
        draft.Poolers.Should().BeEmpty();
        draft.Started.Should().BeFalse();
    }

    [Fact]
    public async Task PostPooler_When_Valid_Should_ReturnOkAndSaveToConfiguredFolder()
    {
        var response = await AddPooler(Alex);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        File.ReadAllText(DraftFiles().Single()).Should().Contain(Alex);
    }

    [Fact]
    public async Task PostPooler_When_Duplicate_Should_ReturnBadRequestWithReason()
    {
        await AddPooler(Alex);

        var response = await AddPooler(Alex.ToLowerInvariant());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Reason(response)).Should().Be("Alex is already in the pooler list.");
    }

    [Fact]
    public async Task PostPooler_When_LimitReached_Should_ReturnBadRequestWithReason()
    {
        await Client.PutAsJsonAsync(SettingsRoute, CustomSettings with { MaxPoolers = 2 });
        await AddPooler(Alex);
        await AddPooler(Sam);

        var response = await AddPooler(Max);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Reason(response)).Should().Be("Cannot add Max: the pooler limit of 2 is reached.");
    }

    [Fact]
    public async Task PutSettings_When_Valid_Should_ReturnUpdatedDraft()
    {
        var response = await Client.PutAsJsonAsync(SettingsRoute, CustomSettings);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<DraftBody>())!.Settings.Should().Be(CustomSettings);
    }

    [Fact]
    public async Task PutSettings_When_DraftStarted_Should_ReturnConflictWithReason()
    {
        SeedStartedDraft();

        var response = await Client.PutAsJsonAsync(SettingsRoute, CustomSettings);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await Reason(response)).Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task PutPooler_When_DraftStarted_Should_Rename()
    {
        SeedStartedDraft();
        var id = (await GetDraft()).Poolers[0].Id;

        var response = await Client.PutAsJsonAsync(PoolerRoute(id), new { name = Alexandre });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await PoolerNames()).Should().Equal(Alexandre);
    }

    [Fact]
    public async Task PutPooler_When_UnknownId_Should_ReturnNotFound()
    {
        var response = await Client.PutAsJsonAsync(PoolerRoute(Guid.NewGuid()), new { name = Sam });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task OrderShuffleDelete_When_NotStarted_Should_Apply()
    {
        foreach (var name in new[] { Alex, Sam, Max })
            await AddPooler(name);

        var ids = (await GetDraft()).Poolers.Select(p => p.Id).Reverse().ToList();

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

        var draft = await GetDraft();
        draft.Started.Should().BeFalse();
        draft.Poolers.Should().ContainSingle();
    }

    [Fact]
    public async Task PostNew_When_DraftStarted_Should_KeepSettingsAndPoolersInNewFile()
    {
        SeedStartedDraft();

        var response = await Client.PostAsync(NewDraftRoute, null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var draft = (await response.Content.ReadFromJsonAsync<DraftBody>())!;
        draft.Started.Should().BeFalse();
        draft.Poolers.Select(p => p.Name).Should().Equal(Alex);
        DraftFiles().Should().HaveCount(2);
    }

    private static string PoolerRoute(Guid id) => $"{PoolersRoute}/{id}";

    private async Task<DraftBody> GetDraft() => (await Client.GetFromJsonAsync<DraftBody>(DraftRoute))!;

    private async Task<IEnumerable<string>> PoolerNames() => (await GetDraft()).Poolers.Select(p => p.Name);

    private Task<HttpResponseMessage> AddPooler(string name) => Client.PostAsJsonAsync(PoolersRoute, new { name });

    private static async Task<string> Reason(HttpResponseMessage response)
    {
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        var problem = (await response.Content.ReadFromJsonAsync<ProblemBody>())!;
        problem.Status.Should().Be((int)response.StatusCode);
        return problem.Detail;
    }

    private void SeedStartedDraft()
    {
        var alex = new Pooler(Guid.NewGuid(), Alex);
        Directory.CreateDirectory(folder);
        var data = new DraftData(new Settings(), [alex], [new Pick(alex.Id, 1, "entry-1")]);
        File.WriteAllText(Path.Combine(folder, "draft-20260101-000000-000.json"), JsonSerializer.Serialize(data, JsonSerializerOptions.Web));
    }

    private string[] DraftFiles() => Directory.GetFiles(folder, "draft-*.json");
}
