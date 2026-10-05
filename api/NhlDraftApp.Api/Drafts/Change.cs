namespace NhlDraftApp.Api.Drafts;

public enum ChangeStatus { Ok, Invalid, Locked, NotFound }

public record Change(ChangeStatus Status, string? Reason = null)
{
    public static readonly Change Ok = new(ChangeStatus.Ok);
    public static readonly Change NotFound = new(ChangeStatus.NotFound, "Pooler not found.");
    public static readonly Change Locked = new(ChangeStatus.Locked, "The draft has started; reset it to change the setup.");

    public static Change Invalid(string reason) => new(ChangeStatus.Invalid, reason);
}
