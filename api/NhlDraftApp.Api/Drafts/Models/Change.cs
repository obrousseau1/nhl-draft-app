namespace NhlDraftApp.Api.Drafts;

public record Change(ChangeStatus Status, string? Reason = null)
{
    public static readonly Change Ok = new(ChangeStatus.Ok);
    public static readonly Change NotFound = new(ChangeStatus.NotFound, "Pooler not found.");
    public static readonly Change Locked = new(ChangeStatus.Locked, "The draft has started: poolers, order and settings are locked. Renaming is still allowed; reset clears all picks.");

    public static Change Invalid(string reason) => new(ChangeStatus.Invalid, reason);
}
