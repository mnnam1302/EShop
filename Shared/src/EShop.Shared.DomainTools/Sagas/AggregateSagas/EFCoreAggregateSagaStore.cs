using EntityFramework.Exceptions.Common;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Shared.DomainTools.Sagas.AggregateSagas;

public sealed class EFCoreAggregateSagaStore : IAggregateSagaStore
{
    private readonly IEventStoreRepository eventStoreRepository;
    private readonly ILogger<EFCoreAggregateSagaStore> _logger;

    public EFCoreAggregateSagaStore(IEventStoreRepository eventStoreRepository, ILogger<EFCoreAggregateSagaStore> logger)
    {
        this.eventStoreRepository = eventStoreRepository;
        _logger = logger;
    }

    public async Task<TSagaAggregate> LoadAggregateSagaAsync<TSagaAggregate>(Guid aggregateSagaId, CancellationToken cancellationToken)
        where TSagaAggregate : IAggregateSaga, new()
    {
        var events = await eventStoreRepository.GetEventStreamAsync(aggregateSagaId, version: 0, cancellationToken: cancellationToken);
        if (events.Count == 0)
        {
            return new TSagaAggregate();
        }

        var aggregate = new TSagaAggregate();
        aggregate.Replay(events);

        return aggregate;
    }

    public async Task UpdateAggregateSagaAsync(IAggregateSaga aggregateSaga, CancellationToken cancellationToken)
    {
        if (!aggregateSaga.UncommittedEvents.Any())
        {
            return;
        }

        var eventDataModels = new List<EventStore>();

        foreach (var @event in aggregateSaga.UncommittedEvents)
        {
            var eventStore = EventStore.Create(aggregateSaga, @event);
            eventDataModels.Add(eventStore);
        }

        _logger.LogTrace(
            "Committing {EventCount} events to PostgreSQL event store for entity with ID '{AggregateId}'",
            eventDataModels.Count,
            aggregateSaga.Id);

        try
        {
            await eventStoreRepository.AppendEventsAsync(eventDataModels, cancellationToken);
            aggregateSaga.MarkEventsAsCommitted();
        }
        catch (DbUpdateException ex) when (ex is UniqueConstraintException)
        {
            _logger.LogTrace(ex,
                "Entity Framework event insert detected an optimistic concurrency exception for entity with ID '{Id}'",
                aggregateSaga.Id);
        }
    }
}
