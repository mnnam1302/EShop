namespace EShop.Finance.Domain.Aggregates.Account.DomainEvents;

public sealed class AccountFailed : AccountDomainEvent
{
    public required Guid AccountId { get; init; }
    public required Guid OrderId { get; init; }
    public required string Reason { get; init; }
}
