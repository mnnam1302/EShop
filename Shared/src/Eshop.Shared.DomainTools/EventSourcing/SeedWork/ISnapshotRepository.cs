namespace EShop.Shared.DomainTools.EventSourcing.SeedWork;

public interface ISnapshotRepository
{
    Task<Snapshot?> GetSnapshotAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddSnapshotAsync(Snapshot snapshot, CancellationToken cancellationToken = default);
}
