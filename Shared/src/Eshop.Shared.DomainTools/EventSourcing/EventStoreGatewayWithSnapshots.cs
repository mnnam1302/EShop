using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using Microsoft.Extensions.Options;

namespace EShop.Shared.DomainTools.EventSourcing;

public sealed class EventStoreGatewayWithSnapshots : EventStoreGatewayBase
{
    private readonly ISnapshotRepository snapshotRepository;

    public EventStoreGatewayWithSnapshots(
        IOptions<EventStoreOptions> options,
        IEventStoreRepository eventStoreRepository,
        ISnapshotRepository snapshotRepository)
        : base(options, eventStoreRepository)
    {
        this.snapshotRepository = snapshotRepository;
    }

    protected override async Task HandleSnapshotCreation(IAggregate aggregate, EventStore eventStore, IDomainEvent @event, CancellationToken cancellationToken)
    {
        if (Options.IncludeSnapshots)
        {
            /*
             * Ex: SnapshotInterval = 3 - Create a snapshot every 3 events
             * @event.Version = 3 => 3 % 3 = 0 => Create snapshot
             */
            if ((long)@event.Version % Options.SnapshotIntervalInEvents == 0)
            {
                var snapshot = Snapshot.Create(aggregate, eventStore);
                await snapshotRepository.AddSnapshotAsync(snapshot, cancellationToken);
            }
        }
    }

    public override async Task<TAggregate> LoadAggregateAsync<TAggregate>(Guid aggregateId, CancellationToken cancellationToken)
    {
        if (Options.IncludeSnapshots)
        {
            var snapshot = await snapshotRepository.GetSnapshotAsync(aggregateId, cancellationToken);

            if (snapshot?.Aggregate is TAggregate snapshotAggregate)
            {
                var aggregate = snapshotAggregate;
                ulong fromVersion = snapshot.Version + 1;

                // Load events after snapshot
                var eventsAfterSnapshot = await EventStoreRepository.GetEventStreamAsync(aggregateId, fromVersion, cancellationToken);
                if (eventsAfterSnapshot.Any())
                {
                    aggregate.LoadFromHistory(eventsAfterSnapshot);
                }

                return aggregate;
            }
        }

        // No snapshot or snapshot loading failed, load from beginning
        return await LoadAggregateFromEvents<TAggregate>(aggregateId, cancellationToken);
    }
}