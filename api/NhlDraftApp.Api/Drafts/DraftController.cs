using Microsoft.AspNetCore.Mvc;
using NhlDraftApp.Api.Drafts.Data;
using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Drafts;

[ApiController]
[Route("api/draft")]
public class DraftController(DraftStore store) : ControllerBase
{
    [HttpGet]
    public DraftView Get() => store.Read();

    [HttpPut("settings")]
    public ActionResult<DraftView> UpdateSettings(Settings settings) => this.Respond(store, d => d.UpdateSettings(settings));

    [HttpPost("new")]
    public DraftView StartNew() => store.StartNew();

    [HttpPost("reset")]
    public ActionResult<DraftView> Reset() => this.Respond(store, d => d.Reset());
}
