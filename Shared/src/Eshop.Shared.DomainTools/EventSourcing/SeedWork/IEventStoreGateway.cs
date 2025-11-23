using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.DomainTools.Exceptions;
using Microsoft.Extensions.Options;

namespace EShop.Shared.DomainTools.EventSourcing.SeedWork;

public interface IEventStoreGateway
{
    Task AppendEventsAsync(IAggregate aggregate, CancellationToken cancellationToken);

    Task<TAggregate> LoadAggregateAsync<TAggregate>(Guid aggregateId, CancellationToken cancellationToken) where TAggregate : IAggregate, new();
}

public abstract class EventStoreGatewayBase : IEventStoreGateway
{
    protected readonly EventStoreOptions Options;
    protected readonly IEventStoreRepository EventStoreRepository;

    protected EventStoreGatewayBase(IOptions<EventStoreOptions> options, IEventStoreRepository eventStoreRepository)
    {
        Options = options.Value;
        EventStoreRepository = eventStoreRepository;
    }

    public virtual async Task AppendEventsAsync(IAggregate aggregate, CancellationToken cancellationToken)
    {
        foreach (var @event in aggregate.UncommittedEvents)
        {
            var eventStore = EventStore.Create(aggregate, @event);
            await EventStoreRepository.AppendEventAsync(eventStore, cancellationToken);
            await HandleSnapshotCreation(aggregate, eventStore, @event, cancellationToken);
        }
    }

    public abstract Task<TAggregate> LoadAggregateAsync<TAggregate>(Guid aggregateId, CancellationToken cancellationToken) where TAggregate : IAggregate, new();

    protected async Task<TAggregate> LoadAggregateFromEvents<TAggregate>(Guid aggregateId, CancellationToken cancellationToken) where TAggregate : IAggregate, new()
    {
        var events = await EventStoreRepository.GetEventStreamAsync(aggregateId, version: 0, cancellationToken: cancellationToken);
        if (events.Count == 0)
        {
            throw new AggregatenNotFoundException(aggregateId, typeof(TAggregate));
        }

        var aggregate = new TAggregate();
        aggregate.LoadFromHistory(events);
        return aggregate;
    }

    protected abstract Task HandleSnapshotCreation(IAggregate aggregate, EventStore eventStore, IDomainEvent @event, CancellationToken cancellationToken);
}