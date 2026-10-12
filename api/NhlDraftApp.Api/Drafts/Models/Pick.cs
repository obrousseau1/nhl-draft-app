namespace NhlDraftApp.Api.Drafts.Models;

public record Pick(Guid PoolerId, int Round, string EntryId)
{
    public bool Fills(Guid poolerId, int round) => PoolerId == poolerId && Round == round;
}
