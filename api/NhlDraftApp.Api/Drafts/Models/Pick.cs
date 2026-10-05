namespace NhlDraftApp.Api.Drafts;

public record Pick(Guid PoolerId, int Round, string EntryId);
