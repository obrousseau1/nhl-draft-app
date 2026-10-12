namespace NhlDraftApp.Api.Drafts.Models;

public record Change(ChangeStatus Status, string? Reason = null)
{
    public static readonly Change Ok = new(ChangeStatus.Ok);
    public static readonly Change NotFound = new(ChangeStatus.NotFound, "Pooler not found.");
    public static readonly Change EntryNotFound = new(ChangeStatus.NotFound, "Player or team not found in the pool.");
    public static readonly Change Locked = new(ChangeStatus.Locked, "The draft has started: poolers and order are locked. Renaming is still allowed; reset clears all picks.");

    public static Change Invalid(string reason) => new(ChangeStatus.Invalid, reason);

    public static Change Refused(string reason) => new(ChangeStatus.Refused, reason);
}
