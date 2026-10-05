using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Drafts;

public record DraftView(Settings Settings, IReadOnlyList<Pooler> Poolers, IReadOnlyList<Pick> Picks, bool Started)
{
    public static DraftView Of(Draft draft) => new(draft.Settings, [.. draft.Poolers], [.. draft.Picks], draft.Started);
}
