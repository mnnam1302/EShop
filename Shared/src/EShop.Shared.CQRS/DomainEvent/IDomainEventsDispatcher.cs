using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Shared.CQRS.DomainEvent;

public interface IDomainEventsDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
