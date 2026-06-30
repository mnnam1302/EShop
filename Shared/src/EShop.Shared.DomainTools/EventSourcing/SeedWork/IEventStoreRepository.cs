using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Shared.DomainTools.EventSourcing.SeedWork;

public interface IEventStoreRepository
{
    Task<IReadOnlyCollection<IDomainEvent>> GetEventStreamAsync(Guid aggregateId, ulong? version = null, CancellationToken cancellationToken = default);
    Task AppendEventAsync(EventStore eventStore, CancellationToken cancellationToken = default);
    Task AppendEventsAsync(IReadOnlyList<EventStore> eventStores, CancellationToken cancellationToken = default);
}
