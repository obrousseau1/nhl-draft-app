using Microsoft.AspNetCore.Mvc;
using NhlDraftApp.Api.Drafts.Data;
using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Drafts;

[ApiController]
[Route("api/poolers")]
public class PoolersController(DraftStore store) : ControllerBase
{
    [HttpPost]
    public ActionResult<DraftView> Add(NameInput input) => this.Respond(store, d => d.AddPooler(input.Name));

    [HttpPut("{id:guid}")]
    public ActionResult<DraftView> Rename(Guid id, NameInput input) => this.Respond(store, d => d.RenamePooler(id, input.Name));

    [HttpDelete("{id:guid}")]
    public ActionResult<DraftView> Remove(Guid id) => this.Respond(store, d => d.RemovePooler(id));

    [HttpPut("order")]
    public ActionResult<DraftView> Reorder(List<Guid> ids) => this.Respond(store, d => d.Reorder(ids));

    [HttpPost("shuffle")]
    public ActionResult<DraftView> Shuffle() => this.Respond(store, d => d.Shuffle(Random.Shared));
}
