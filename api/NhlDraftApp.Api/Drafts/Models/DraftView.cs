using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Drafts.Models;

public record DraftView(Settings Settings, IReadOnlyList<Pooler> Poolers, IReadOnlyList<Pick> Picks, bool Started, IReadOnlyList<BoardRow> Board, Box? CurrentPick)
{
    public static DraftView Of(Draft draft) => new(
        draft.Settings, [.. draft.Poolers], [.. draft.Picks], draft.Started, [.. draft.Poolers.Select(p => BoardRow.Of(draft, p))], Snake.Current(draft));
}
