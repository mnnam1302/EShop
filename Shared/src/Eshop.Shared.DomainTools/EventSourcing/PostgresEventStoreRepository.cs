using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using Microsoft.EntityFrameworkCore;

namespace EShop.Shared.DomainTools.EventSourcing;

public sealed class PostgresEventStoreRepository<TDbContext> : IEventStoreRepository
    where TDbContext : DbContext, IEventStoreDbContext
{
    private readonly TDbContext _dbContext;

    public PostgresEventStoreRepository(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<IDomainEvent>> GetEventStreamAsync(Guid aggregateId, ulong? version = null, CancellationToken cancellationToken = default)
    {
        return await _dbContext.EventStores
            .AsNoTracking()
            .Where(es => es.AggregateId == aggregateId && es.Version > (version ?? 0))
            .OrderBy(es => es.Version)
            .Select(es => es.Event)
            .ToListAsync(cancellationToken);
    }

    public async Task AppendEventAsync(EventStore eventStore, CancellationToken cancellationToken = default)
    {
        _dbContext.EventStores.Add(eventStore);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
