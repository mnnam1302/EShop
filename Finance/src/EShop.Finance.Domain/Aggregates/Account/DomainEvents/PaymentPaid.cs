namespace EShop.Finance.Domain.Aggregates.Account.DomainEvents;

public sealed class PaymentPaid : AccountDomainEvent
{
    public required Guid AccountId { get; init; }
    public required Guid PaymentId { get; init; }
    public required decimal Amount { get; init; }
}
