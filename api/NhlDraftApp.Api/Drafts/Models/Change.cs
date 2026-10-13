namespace NhlDraftApp.Api.Drafts.Models;

// Code names the rule behind a refusal, so callers can react without parsing Reason.
public record Change(ChangeStatus Status, string? Reason = null, Enum? Code = null)
{
    public static readonly Change Ok = new(ChangeStatus.Ok);
    public static readonly Change NotFound = new(ChangeStatus.NotFound, "Pooler not found.");
    public static readonly Change EntryNotFound = new(ChangeStatus.NotFound, "Player or team not found in the pool.");
    public static readonly Change Locked = new(ChangeStatus.Locked, "The draft has started: poolers and order are locked. Renaming is still allowed; reset clears all picks.");

    public static Change Invalid(string reason) => new(ChangeStatus.Invalid, reason);

    public static Change Invalid(SettingsMisfit misfit) => new(ChangeStatus.Invalid, misfit.Message, misfit.Rule);

    public static Change Refused(PickRefusal refusal) => new(ChangeStatus.Refused, refusal.Message, refusal.Rule);
}
