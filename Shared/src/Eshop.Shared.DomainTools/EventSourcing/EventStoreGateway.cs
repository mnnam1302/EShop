using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using EShop.Shared.DomainTools.Exceptions;
using Microsoft.Extensions.Options;

namespace EShop.Shared.DomainTools.EventSourcing;

public sealed class EventStoreGateway : IEventStoreGateway
{
    private readonly EventStoreOptions options;
    private readonly IEventStoreRepository eventStoreRepository;
    private readonly ISnapshotRepository snapshotRepository;

    public EventStoreGateway(IOptions<EventStoreOptions> options, IEventStoreRepository eventStoreRepository, ISnapshotRepository? snapshotRepository)
    {
        this.options = options.Value;
        this.eventStoreRepository = eventStoreRepository;
        this.snapshotRepository = snapshotRepository ?? new NullSnapshotRepository();
    }

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

                    if (eventsAfterSnapshot.Any())
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
            throw new AggregatenNotFoundException(aggregateId, typeof(TAggregate));
        }

        var aggregate = new TAggregate();
        aggregate.Replay(events);

        return aggregate;
    }

    public async Task AppendEventsAsync(IAggregate aggregate, CancellationToken cancellationToken)
    {
        foreach (var @event in aggregate.UncommittedEvents)
        {
            var eventStore = EventStore.Create(aggregate, @event);
            await eventStoreRepository.AppendEventAsync(eventStore, cancellationToken);

            if (options.IncludeSnapshots)
            {
                /*
                 * Ex: SnapshotInterval = 3 - Create a snapshot every 3 events
                 * @event.Version = 3 => 3 % 3 = 0 => Create snapshot
                 */
                if ((long)@event.Version % options.SnapshotIntervalInEvents == 0)
                {
                    var snapshot = Snapshot.Create(aggregate, eventStore);
                    await snapshotRepository.AddSnapshotAsync(snapshot, cancellationToken);
                }
            }
        }
    }
}