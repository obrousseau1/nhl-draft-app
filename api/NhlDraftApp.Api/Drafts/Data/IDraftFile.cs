using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Drafts.Data;

public interface IDraftFile
{
    DraftData? Load();

    void Save(DraftData data);

    void SaveAsNew(DraftData data);
}
