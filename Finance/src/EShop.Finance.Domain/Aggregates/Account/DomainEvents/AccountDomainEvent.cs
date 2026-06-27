using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Finance.Domain.Aggregates.Account.DomainEvents;

public abstract class AccountDomainEvent : IDomainEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTimeOffset TimeStampUtc { get; set; } = DateTimeOffset.UtcNow;
    public ulong Version { get; set; }
}
