using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Shared.Contracts.Services.Order.Saga;

/// <summary>
/// Finance's reply to <see cref="MakePayment"/> when the account/schedule could not be created
/// (e.g. invalid total or frequency). The Order saga consumes this to compensate (release the
/// inventory reservation and fail the order).
/// </summary>
public sealed class OrderPaymentScheduleFailed : IntegrationEvent
{
    public required Guid OrderId { get; init; }
    public required string Reason { get; init; }
}
