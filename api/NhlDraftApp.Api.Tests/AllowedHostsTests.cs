using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace NhlDraftApp.Api.Tests;

public sealed class AllowedHostsTests : IDisposable
{
    private const string DraftRoute = "/api/draft";

    private readonly string folder = Path.Combine(Path.GetTempPath(), "nhl-draft-tests", Guid.NewGuid().ToString());
    private readonly WebApplicationFactory<Program> factory;

    public AllowedHostsTests()
    {
        factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b => b.UseSetting("Draft:DataFolder", folder));
    }

    public void Dispose()
    {
        factory.Dispose();
        if (Directory.Exists(folder))
            Directory.Delete(folder, recursive: true);
    }

    [Theory]
    [InlineData("localhost")]
    [InlineData("127.0.0.1:5190")]
    public async Task Request_When_HostIsLocal_Should_Pass(string host)
    {
        var response = await Get(host);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Request_When_HostIsForeign_Should_ReturnBadRequest()
    {
        var response = await Get("evil.example:5190");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private Task<HttpResponseMessage> Get(string host)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, DraftRoute);
        request.Headers.Host = host;
        return factory.CreateClient().SendAsync(request);
    }
}
