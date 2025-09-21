using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Authorization.Domain.DomainEvents;

public class RootOrganizationCreatedDomainEvent : IDomainEvent
{
    public DateTimeOffset TimeStamp => DateTimeOffset.UtcNow;
}
