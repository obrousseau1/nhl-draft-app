namespace NhlDraftApp.Api.Hosting;

// Serves the built React app (web/ builds into wwwroot) so draft night needs only `dotnet run`, offline.
public static class WebAppHosting
{
    public static WebApplication MapWebApp(this WebApplication app)
    {
        app.UseStaticFiles();
        app.MapFallback("/api/{**path}", context =>
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        });
        app.MapFallbackToFile("index.html");
        return app;
    }
}
