using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Drafts.Data;

public class DraftStore(IDraftFile file)
{
    private readonly Lock gate = new();
    private Draft draft = new(file.Load());

    public DraftView Read()
    {
        lock (gate)
            return DraftView.Of(draft);
    }

    public DraftView StartNew()
    {
        lock (gate)
        {
            var next = new Draft(draft.ToData() with { Picks = [] });
            file.SaveAsNew(next.ToData());
            draft = next;
            return DraftView.Of(draft);
        }
    }

    public Applied Apply(Func<Draft, Change> change)
    {
        lock (gate)
        {
            var copy = new Draft(draft.ToData());
            var result = change(copy);
            if (result.Status != ChangeStatus.Ok)
                return new Applied(result, DraftView.Of(draft));
            file.Save(copy.ToData());
            draft = copy;
            return new Applied(result, DraftView.Of(draft));
        }
    }
}
