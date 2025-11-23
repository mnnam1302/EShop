using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.DomainTools.EventSourcing.SeedWork;
using Microsoft.Extensions.Options;

namespace EShop.Shared.DomainTools.EventSourcing;

public sealed class EventStoreGateway : EventStoreGatewayBase
{
    public EventStoreGateway(IOptions<EventStoreOptions> options, IEventStoreRepository eventStoreRepository)
        : base(options, eventStoreRepository)
    {
    }

    protected override Task HandleSnapshotCreation(IAggregate aggregate, EventStore eventStore, IDomainEvent @event, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public override async Task<TAggregate> LoadAggregateAsync<TAggregate>(Guid aggregateId, CancellationToken cancellationToken)
    {
        return await LoadAggregateFromEvents<TAggregate>(aggregateId, cancellationToken);
    }
}