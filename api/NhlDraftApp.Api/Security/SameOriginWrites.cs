namespace NhlDraftApp.Api.Security;

// Blocks cross-site writes: browsers always send Origin on cross-origin requests, and body-less POSTs skip the CORS preflight.
public static class SameOriginWrites
{
    public static IApplicationBuilder UseSameOriginWrites(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            if (IsForeignWrite(context.Request))
            {
                await Results.Problem(detail: "Cross-site requests are not allowed.", statusCode: StatusCodes.Status403Forbidden)
                    .ExecuteAsync(context);
                return;
            }
            await next();
        });

    private static bool IsForeignWrite(HttpRequest request)
    {
        var origin = request.Headers.Origin.ToString();
        if (HttpMethods.IsGet(request.Method) || origin.Length == 0)
            return false;
        return !Uri.TryCreate(origin, UriKind.Absolute, out var uri) || uri.Authority != request.Host.Value;
    }
}
