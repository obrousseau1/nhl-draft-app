using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Tests.Security;

public sealed class SameOriginWritesTests : IDisposable
{
    private const string Alex = "Alex";
    private const string DraftRoute = "/api/draft";
    private const string PoolersRoute = "/api/poolers";
    private const string ShuffleRoute = "/api/poolers/shuffle";
    private const string ForeignOrigin = "https://evil.example";
    private const string SameOrigin = "http://localhost";

    private readonly string folder = Path.Combine(Path.GetTempPath(), "nhl-draft-tests", Guid.NewGuid().ToString());
    private readonly WebApplicationFactory<Program> factory;
    private HttpClient? client;

    public SameOriginWritesTests()
    {
        factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b => b.UseSetting("Draft:DataFolder", folder));
    }

    private HttpClient Client => client ??= factory.CreateClient();

    public void Dispose()
    {
        client?.Dispose();
        factory.Dispose();
        if (Directory.Exists(folder))
            Directory.Delete(folder, recursive: true);
    }

    [Theory]
    [InlineData(ForeignOrigin)]
    [InlineData("null")]
    public async Task Write_When_OriginIsForeign_Should_ReturnForbiddenAndChangeNothing(string origin)
    {
        var response = await Send(HttpMethod.Post, PoolersRoute, origin, JsonContent.Create(new { name = Alex }));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        (await response.Content.ReadAsStringAsync()).Should().Contain("Cross-site requests are not allowed.");
        (await Client.GetFromJsonAsync<DraftView>(DraftRoute))!.Poolers.Should().BeEmpty();
    }

    [Fact]
    public async Task Write_When_BodylessPostFromForeignOrigin_Should_ReturnForbidden()
    {
        var response = await Send(HttpMethod.Post, ShuffleRoute, ForeignOrigin);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Write_When_OriginMatchesHost_Should_Pass()
    {
        var response = await Send(HttpMethod.Post, PoolersRoute, SameOrigin, JsonContent.Create(new { name = Alex }));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Read_When_OriginIsForeign_Should_Pass()
    {
        var response = await Send(HttpMethod.Get, DraftRoute, ForeignOrigin);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private Task<HttpResponseMessage> Send(HttpMethod method, string route, string origin, HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, route) { Content = content };
        request.Headers.Add("Origin", origin);
        return Client.SendAsync(request);
    }
}
