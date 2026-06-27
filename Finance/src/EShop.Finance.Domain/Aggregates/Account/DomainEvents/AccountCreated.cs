namespace EShop.Finance.Domain.Aggregates.Account.DomainEvents;

public sealed class AccountCreated : AccountDomainEvent
{
    public required Guid AccountId { get; init; }
    public required Guid OrderId { get; init; }
    public required string TenantId { get; init; }
}
