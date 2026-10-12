using NhlDraftApp.Api.Drafts;
using NhlDraftApp.Api.Drafts.Models;

namespace NhlDraftApp.Api.Tests.Drafts;

internal static class TestPool
{
    public static PoolEntry Player(string last, Position position, decimal capHit) =>
        new($"id-{last}", string.Empty, last, "EDM", position, capHit, NoCapHit: false);

    public static PoolEntry Team(string name) =>
        new($"id-{name}", name, string.Empty, name, Position.Team, 0m, NoCapHit: false);

    public static Draft Create(Settings settings, IEnumerable<PoolEntry> pool, params string[] names)
    {
        var poolers = names.Select(n => new Pooler(Guid.NewGuid(), n)).ToList();
        return new Draft(new DraftData(settings, poolers, [], [.. pool]));
    }

    public static void PickInOrder(this Draft draft, int poolerIndex, params PoolEntry[] entries)
    {
        for (var i = 0; i < entries.Length; i++)
            draft.Pick(draft.Poolers[poolerIndex].Id, i + 1, entries[i].Id).Should().Be(Change.Ok);
    }
}
