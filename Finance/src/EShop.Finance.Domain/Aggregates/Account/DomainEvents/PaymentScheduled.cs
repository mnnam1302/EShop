namespace EShop.Finance.Domain.Aggregates.Account.DomainEvents;

public sealed class PaymentScheduled : AccountDomainEvent
{
    public required Guid AccountId { get; init; }
    public required Guid OrderId { get; init; }
    public required int PaymentCount { get; init; }
}
