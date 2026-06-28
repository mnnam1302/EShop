namespace EShop.Finance.Domain.Aggregates.Account.DomainEvents;

public sealed class AccountCompleted : AccountDomainEvent
{
    public required Guid AccountId { get; init; }
    public required Guid OrderId { get; init; }
}
