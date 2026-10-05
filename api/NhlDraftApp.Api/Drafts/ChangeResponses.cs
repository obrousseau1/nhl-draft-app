namespace NhlDraftApp.Api.Drafts;

public static class ChangeResponses
{
    public static IResult Respond(DraftStore store, Func<Draft, Change> change)
    {
        var result = store.Apply(change);
        return result.Status == ChangeStatus.Ok ? Results.Ok(store.Read(DraftView.Of)) : Refusal(result);
    }

    public static IResult Refusal(Change change)
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
