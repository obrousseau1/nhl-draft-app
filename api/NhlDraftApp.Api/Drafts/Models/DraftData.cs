namespace NhlDraftApp.Api.Drafts;

public record DraftData(Settings Settings, List<Pooler> Poolers, List<Pick> Picks);
