using Microsoft.AspNetCore.Mvc;
using NhlDraftApp.Api.Drafts.Data;
using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Drafts;

public static class ChangeResponses
{
    public static ActionResult<DraftView> Respond(this ControllerBase controller, DraftStore store, Func<Draft, Change> change)
    {
        var applied = store.Apply(change);
        return applied.Change.Status == ChangeStatus.Ok ? controller.Ok(applied.View) : controller.Refusal(applied.Change);
    }

    private static ObjectResult Refusal(this ControllerBase controller, Change change)
    {
        var status = change.Status switch
        {
            ChangeStatus.Locked => StatusCodes.Status409Conflict,
            ChangeStatus.NotFound => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status400BadRequest,
        };
        return controller.Problem(detail: change.Reason, statusCode: status);
    }
}
