using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using NhlDraftApp.Api.Drafts;
using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Tests;

public sealed class PicksEndpointsTests : IDisposable
{
    private const string Alex = "Alex";
    private const string Sam = "Sam";
    private const string DraftRoute = "/api/draft";

    private static readonly Settings ThreeRounds = new(Rounds: 3, DefenseMin: 0, Goalies: 0, Teams: 0);
    private static readonly PoolEntry McDavid = new("id-mcdavid", "Connor", "McDavid", "EDM", Position.C, 12.5m, NoCapHit: false);
    private static readonly PoolEntry Makar = new("id-makar", "Cale", "Makar", "COL", Position.D, 9m, NoCapHit: false);

    private readonly string folder = Path.Combine(Path.GetTempPath(), "nhl-draft-tests", Guid.NewGuid().ToString());
    private readonly Pooler alex = new(Guid.NewGuid(), Alex);
    private readonly Pooler sam = new(Guid.NewGuid(), Sam);
    private WebApplicationFactory<Program> factory;
    private HttpClient client;

    public PicksEndpointsTests()
    {
        Directory.CreateDirectory(folder);
        var data = new DraftData(ThreeRounds, [alex, sam], [], [McDavid, Makar]);
        File.WriteAllText(Path.Combine(folder, "draft-20260101-000000-000.json"), JsonSerializer.Serialize(data, JsonSerializerOptions.Web));
        (factory, client) = StartHost();
    }

    public void Dispose()
    {
        StopHost();
        if (Directory.Exists(folder))
            Directory.Delete(folder, recursive: true);
    }

    [Fact]
    public async Task PutPick_When_PickAllowed_Should_ReturnOkWithThePick()
    {
        var response = await PutPick(alex, 1, McDavid);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadFromJsonAsync<DraftView>())!.Picks.Should().Equal(new Pick(alex.Id, 1, McDavid.Id));
    }

    [Fact]
    public async Task GetDraft_When_PickMade_Should_ListRowsInPoolerOrder()
    {
        var draft = await DraftAfterFirstPick();

        draft.Board.Select(r => r.PoolerId).Should().Equal(alex.Id, sam.Id);
    }

    [Fact]
    public async Task GetDraft_When_PickMade_Should_GiveEachRowOneBoxPerRound()
    {
        var draft = await DraftAfterFirstPick();

        draft.Board.Should().AllSatisfy(r => r.Boxes.Select(b => b.Round).Should().Equal(1, 2, 3));
    }

    [Fact]
    public async Task GetDraft_When_PickMade_Should_FillTheBoxWithNamePositionTeamAndCap()
    {
        var draft = await DraftAfterFirstPick();

        var box = draft.Board[0].Boxes[0].Entry!;
        (box.Name, box.Position, box.NhlTeam, box.CapHit).Should().Be(("Connor McDavid", Position.C, "EDM", 12.5m));
    }

    [Fact]
    public async Task GetDraft_When_PickMade_Should_LeaveOtherPoolersBoxesEmpty()
    {
        var draft = await DraftAfterFirstPick();

        draft.Board[1].Boxes.Should().AllSatisfy(b => b.Entry.Should().BeNull());
    }

    [Fact]
    public async Task GetDraft_When_PickMade_Should_MoveCurrentPickToTheNextPooler()
    {
        var draft = await DraftAfterFirstPick();

        draft.CurrentPick.Should().Be(new Box(sam.Id, 1));
    }

    [Fact]
    public async Task PutPick_When_RuleRefuses_Should_ReturnUnprocessableWithReasonAndCode()
    {
        await PutPick(alex, 1, McDavid);

        var response = await PutPick(sam, 1, McDavid);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        problem.GetProperty("detail").GetString().Should().Contain(Alex);
        problem.GetProperty("code").GetString().Should().Be(nameof(PickRule.Taken));
    }

    [Fact]
    public async Task DeletePick_When_BoxFilled_Should_EmptyTheBox()
    {
        await PutPick(alex, 1, McDavid);

        var response = await client.DeleteAsync(PickRoute(alex, 1));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await GetDraft()).Picks.Should().BeEmpty();
    }

    [Fact]
    public async Task DeletePick_When_PoolerUnknown_Should_ReturnNotFound()
    {
        var response = await client.DeleteAsync(PickRoute(new Pooler(Guid.NewGuid(), Sam), 1));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDraft_When_HostRestarted_Should_ReturnTheSamePicksTotalsAndCurrentPick()
    {
        await PutPick(alex, 1, McDavid);
        await PutPick(sam, 2, Makar);
        var before = await client.GetStringAsync(DraftRoute);

        StopHost();
        (factory, client) = StartHost();

        (await client.GetStringAsync(DraftRoute)).Should().Be(before);
        Directory.GetFiles(folder, "*.tmp").Should().BeEmpty();
    }

    private static string PickRoute(Pooler pooler, int round) => $"/api/picks/{pooler.Id}/{round}";

    private (WebApplicationFactory<Program> Factory, HttpClient Client) StartHost()
    {
        var host = new WebApplicationFactory<Program>().WithWebHostBuilder(b => b.UseSetting("Draft:DataFolder", folder));
        return (host, host.CreateClient());
    }

    private void StopHost()
    {
        client.Dispose();
        factory.Dispose();
    }

    private Task<HttpResponseMessage> PutPick(Pooler pooler, int round, PoolEntry entry) =>
        client.PutAsJsonAsync(PickRoute(pooler, round), new { entryId = entry.Id });

    private async Task<DraftView> GetDraft() => (await client.GetFromJsonAsync<DraftView>(DraftRoute))!;

    private async Task<DraftView> DraftAfterFirstPick()
    {
        await PutPick(alex, 1, McDavid);
        return await GetDraft();
    }
}
