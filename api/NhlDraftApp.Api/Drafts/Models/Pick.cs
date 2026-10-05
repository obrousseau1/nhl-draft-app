namespace NhlDraftApp.Api.Drafts.Models;

public record Pick(Guid PoolerId, int Round, string EntryId);
