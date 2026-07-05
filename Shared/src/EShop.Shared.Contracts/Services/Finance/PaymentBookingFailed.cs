using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Shared.Contracts.Services.Finance;

public sealed class PaymentBookingFailed : IntegrationEvent
{
    public required Guid AccountId { get; init; }
    public required Guid PaymentId { get; init; }
    public required string Reason { get; init; }
}
