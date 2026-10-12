namespace NhlDraftApp.Api.Drafts.Models;

public record PoolEntry(string Id, string First, string Last, string NhlTeam, Position Position, decimal CapHit, bool NoCapHit)
{
    public string Name => $"{First} {Last}".Trim();
}
