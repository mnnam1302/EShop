namespace EShop.Finance.Domain.Aggregates.Account.DomainEvents;

public sealed class PaymentBooked : AccountDomainEvent
{
    public required Guid AccountId { get; init; }
    public required Guid PaymentId { get; init; }
    public required string ExternalReference { get; init; }
}
