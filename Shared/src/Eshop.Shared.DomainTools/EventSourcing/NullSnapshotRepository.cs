using EShop.Shared.DomainTools.EventSourcing.SeedWork;

namespace EShop.Shared.DomainTools.EventSourcing;

public sealed class NullSnapshotRepository : ISnapshotRepository
{
    public Task AddSnapshotAsync(Snapshot snapshot, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<Snapshot?> GetSnapshotAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Snapshot?>(null);
    }
}
