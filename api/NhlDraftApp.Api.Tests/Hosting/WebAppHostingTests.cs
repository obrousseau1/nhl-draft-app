using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace NhlDraftApp.Api.Tests.Hosting;

public sealed class WebAppHostingTests : IDisposable
{
    private const string IndexHtml = "<!doctype html><div id=\"root\"></div>";
    private const string DraftRoute = "/api/draft";

    private readonly string root = Path.Combine(Path.GetTempPath(), "nhl-draft-tests", Guid.NewGuid().ToString());
    private readonly string webRoot;
    private readonly WebApplicationFactory<Program> factory;

    public WebAppHostingTests()
    {
        webRoot = Path.Combine(root, "wwwroot");
        Directory.CreateDirectory(webRoot);
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b => b
            .UseSetting("Draft:DataFolder", Path.Combine(root, "data"))
            .UseWebRoot(webRoot)
            .UseEnvironment("Testing"));
    }

    public void Dispose()
    {
        factory.Dispose();
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/board/round/3")]
    public async Task Get_When_WebAppBuilt_Should_ServeIndex(string route)
    {
        File.WriteAllText(Path.Combine(webRoot, "index.html"), IndexHtml);

        var response = await factory.CreateClient().GetAsync(route);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Be(IndexHtml);
    }

    [Fact]
    public async Task Get_When_AssetExists_Should_ServeIt()
    {
        Directory.CreateDirectory(Path.Combine(webRoot, "assets"));
        File.WriteAllText(Path.Combine(webRoot, "assets", "app.js"), "console.log(1)");

        var response = await factory.CreateClient().GetAsync("/assets/app.js");

        (await response.Content.ReadAsStringAsync()).Should().Be("console.log(1)");
    }

    [Fact]
    public async Task Get_When_UnknownApiRoute_Should_ReturnNotFound()
    {
        File.WriteAllText(Path.Combine(webRoot, "index.html"), IndexHtml);

        var response = await factory.CreateClient().GetAsync("/api/unknown");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_When_WebAppNotBuilt_Should_StillServeApi()
    {
        var client = factory.CreateClient();

        (await client.GetAsync("/")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await client.GetAsync(DraftRoute)).StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
