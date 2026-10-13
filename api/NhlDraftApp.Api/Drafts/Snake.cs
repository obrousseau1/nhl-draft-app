using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Drafts;

public static class Snake
{
    public static Box? Current(Draft draft)
    {
        for (var round = 1; round <= draft.Settings.Rounds; round++)
        {
            var order = round % 2 == 1 ? draft.Poolers : draft.Poolers.Reverse();
            var open = order.FirstOrDefault(p => !draft.Picks.Any(k => k.Fills(p.Id, round)));
            if (open is not null)
                return new Box(open.Id, round);
        }

        return null;
    }
}
