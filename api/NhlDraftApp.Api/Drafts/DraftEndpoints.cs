namespace NhlDraftApp.Api.Drafts;

public record NameInput(string Name);

public record DraftView(Settings Settings, IReadOnlyList<Pooler> Poolers, IReadOnlyList<Pick> Picks, bool Started)
{
    public static DraftView Of(Draft draft) => new(draft.Settings, [.. draft.Poolers], [.. draft.Picks], draft.Started);
}

public static class DraftEndpoints
{
    public static void MapDraft(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/draft", (DraftStore store) => store.Read(DraftView.Of));

        app.MapPut("/api/draft/settings", (Settings settings, DraftStore store) =>
            Respond(store, d => d.UpdateSettings(settings)));

        app.MapPost("/api/draft/reset", (DraftStore store) => Respond(store, d => d.Reset()));

        var poolers = app.MapGroup("/api/poolers");

        poolers.MapPost("/", (NameInput input, DraftStore store) =>
        {
            var change = store.Apply(d => d.AddPooler(input.Name));
            return change.Status == ChangeStatus.Ok
                ? Results.Created("/api/draft", store.Read(DraftView.Of))
                : Refusal(change);
        });

        poolers.MapPut("/{id:guid}", (Guid id, NameInput input, DraftStore store) =>
            Respond(store, d => d.RenamePooler(id, input.Name)));

        poolers.MapDelete("/{id:guid}", (Guid id, DraftStore store) => Respond(store, d => d.RemovePooler(id)));

        poolers.MapPut("/order", (List<Guid> ids, DraftStore store) => Respond(store, d => d.Reorder(ids)));

        poolers.MapPost("/shuffle", (DraftStore store) => Respond(store, d => d.Shuffle(Random.Shared)));
    }

    private static IResult Respond(DraftStore store, Func<Draft, Change> change)
    {
        var result = store.Apply(change);
        return result.Status == ChangeStatus.Ok ? Results.Ok(store.Read(DraftView.Of)) : Refusal(result);
    }

    private static IResult Refusal(Change change)
    {
        var body = new { reason = change.Reason };
        return change.Status switch
        {
            ChangeStatus.Locked => Results.Conflict(body),
            ChangeStatus.NotFound => Results.NotFound(body),
            _ => Results.BadRequest(body),
        };
    }
}
