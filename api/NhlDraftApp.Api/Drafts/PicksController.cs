using Microsoft.AspNetCore.Mvc;
using NhlDraftApp.Api.Drafts.Data;
using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Drafts;

[ApiController]
[Route("api/picks")]
public class PicksController(DraftStore store) : ControllerBase
{
    [HttpPut("{poolerId:guid}/{round:int}")]
    public ActionResult<DraftView> Set(Guid poolerId, int round, PickInput input) => this.Respond(store, d => d.Pick(poolerId, round, input.EntryId));

    [HttpDelete("{poolerId:guid}/{round:int}")]
    public ActionResult<DraftView> Clear(Guid poolerId, int round) => this.Respond(store, d => d.ClearPick(poolerId, round));
}
