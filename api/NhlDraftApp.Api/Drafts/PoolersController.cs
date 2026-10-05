using Microsoft.AspNetCore.Mvc;
using static NhlDraftApp.Api.Drafts.ChangeResponses;

namespace NhlDraftApp.Api.Drafts;

[ApiController]
[Route("api/poolers")]
public class PoolersController(DraftStore store) : ControllerBase
{
    [HttpPost]
    public IResult Add(NameInput input)
    {
        var change = store.Apply(d => d.AddPooler(input.Name));
        return change.Status == ChangeStatus.Ok
            ? Results.Created("/api/draft", store.Read(DraftView.Of))
            : Refusal(change);
    }

    [HttpPut("{id:guid}")]
    public IResult Rename(Guid id, NameInput input) => Respond(store, d => d.RenamePooler(id, input.Name));

    [HttpDelete("{id:guid}")]
    public IResult Remove(Guid id) => Respond(store, d => d.RemovePooler(id));

    [HttpPut("order")]
    public IResult Reorder(List<Guid> ids) => Respond(store, d => d.Reorder(ids));

    [HttpPost("shuffle")]
    public IResult Shuffle() => Respond(store, d => d.Shuffle(Random.Shared));
}
