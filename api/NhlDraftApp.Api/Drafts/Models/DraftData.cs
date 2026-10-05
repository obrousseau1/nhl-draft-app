namespace NhlDraftApp.Api.Drafts.Models;

public record DraftData(Settings Settings, List<Pooler> Poolers, List<Pick> Picks);
