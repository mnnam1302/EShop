using EntityFramework.Exceptions.Common;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using EShop.Shared.DomainTools.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EShop.Shared.DomainTools.EventSourcing;

public sealed class AggregateStore(
    IOptions<EventStoreOptions> options,
    IEventStoreRepository eventStoreRepository,
    ISnapshotRepository? snapshotRepository,
    ILogger<AggregateStore> logger) : IAggregateStore
{
    private readonly EventStoreOptions options = options.Value;
    private readonly ISnapshotRepository snapshotRepository = snapshotRepository ?? new NullSnapshotRepository();

    public async Task<TAggregate> LoadAggregateAsync<TAggregate>(Guid aggregateId, CancellationToken cancellationToken)
        where TAggregate : IAggregate, new()
    {
        if (options.IncludeSnapshots)
        {
            var snapshot = await snapshotRepository.GetSnapshotAsync(aggregateId, cancellationToken);

            if (snapshot != null)
            {
                if (snapshot.Aggregate is TAggregate snapshotAggregate)
                {
                    var fromVersion = snapshot.Version + 1;

                    // Load events after snapshot
                    var eventsAfterSnapshot = await eventStoreRepository.GetEventStreamAsync(aggregateId, fromVersion, cancellationToken);

                    if (eventsAfterSnapshot.Count != 0)
                    {
                        snapshotAggregate.Replay(eventsAfterSnapshot);
                    }

                    return snapshotAggregate;
                }
            }
        }

        var events = await eventStoreRepository.GetEventStreamAsync(aggregateId, version: 0, cancellationToken: cancellationToken);
        if (events.Count == 0)
        {
            throw new AggregateNotFoundException(aggregateId, typeof(TAggregate));
        }

        var aggregate = new TAggregate();
        aggregate.Replay(events);

        return aggregate;
    }

    public async Task AppendEventsAsync(IAggregate aggregate, CancellationToken cancellationToken)
    {
        if (!aggregate.UncommittedEvents.Any())
        {
            return;
        }

        var eventDataModels = new List<EventStore>();
        var snapshotsDataModels = new List<Snapshot>();

        foreach (var @event in aggregate.UncommittedEvents)
        {
            var eventStore = EventStore.Create(aggregate, @event);
            eventDataModels.Add(eventStore);

            if (options.IncludeSnapshots)
            {
                /*
                 * Ex: SnapshotInterval = 3 - Create a snapshot every 3 events
                 * @event.Version = 3 => 3 % 3 = 0 => Create snapshot
                 */
                if ((long)@event.Version % options.SnapshotIntervalInEvents != 0)
                {
                    continue;
                }

                var snapshot = Snapshot.Create(aggregate, eventStore);
                snapshotsDataModels.Add(snapshot);
            }
        }

        logger.LogTrace(
            "Committing {EventCount} events to PostgreSQL event store for entity with ID '{AggregateId}'",
            eventDataModels.Count,
            aggregate.Id);

        try
        {
            await eventStoreRepository.AppendEventsAsync(eventDataModels, cancellationToken);
            aggregate.MarkEventsAsCommitted();

            if (snapshotsDataModels.Count != 0)
            {
                await snapshotRepository.AddSnapShotsAsync(snapshotsDataModels, cancellationToken);
            }
        }
        catch (DbUpdateException ex) when (ex is UniqueConstraintException)
        {
            logger.LogTrace(ex,
                "Entity Framework event insert detected an optimistic concurrency exception for entity with ID '{Id}'",
                aggregate.Id);
        }
    }
}
