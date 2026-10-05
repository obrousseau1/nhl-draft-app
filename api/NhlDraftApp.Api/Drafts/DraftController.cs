using Microsoft.AspNetCore.Mvc;
using static NhlDraftApp.Api.Drafts.ChangeResponses;

namespace NhlDraftApp.Api.Drafts;

[ApiController]
[Route("api/draft")]
public class DraftController(DraftStore store) : ControllerBase
{
    [HttpGet]
    public DraftView Get() => store.Read(DraftView.Of);

    [HttpPut("settings")]
    public IResult UpdateSettings(Settings settings) => Respond(store, d => d.UpdateSettings(settings));

    [HttpPost("reset")]
    public IResult Reset() => Respond(store, d => d.Reset());
}
