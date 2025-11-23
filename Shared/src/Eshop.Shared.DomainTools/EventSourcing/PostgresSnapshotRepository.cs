using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace EShop.Shared.DomainTools.EventSourcing;

public sealed class PostgresSnapshotRepository<TDbContext> : ISnapshotRepository
    where TDbContext : DbContext, ISnapshotDbContext
{
    private readonly TDbContext _dbContext;

    public PostgresSnapshotRepository(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Snapshot?> GetSnapshotAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Snapshots
            .AsNoTracking()
            .Where(s => s.AggregateId == id)
            .OrderByDescending(s => s.Version)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddSnapshotAsync(Snapshot snapshot, CancellationToken cancellationToken = default)
    {
        _dbContext.Snapshots.Add(snapshot);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
