namespace NhlDraftApp.Api.Drafts.Models;

public record BoardRow(Guid PoolerId, IReadOnlyList<BoardBox> Boxes, PoolerTotals Totals)
{
    public static BoardRow Of(Draft draft, Pooler pooler)
    {
        var boxes = Enumerable.Range(1, draft.Settings.Rounds).Select(round => new BoardBox(round, draft.EntryIn(pooler.Id, round)));
        return new BoardRow(pooler.Id, [.. boxes], PoolerTotals.Of(draft, pooler));
    }
}
