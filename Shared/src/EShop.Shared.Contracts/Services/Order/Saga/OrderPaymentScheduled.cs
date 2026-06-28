using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Shared.Contracts.Services.Order.Saga;

/// <summary>
/// Finance's reply to <see cref="MakePayment"/>: the finance account was created and a payment
/// schedule was calculated. The Order saga consumes this to advance out of ProcessingPayment.
/// (Actual collection/booking is a later ticket.)
/// </summary>
public sealed class OrderPaymentScheduled : IntegrationEvent
{
    public required Guid OrderId { get; init; }
    public required Guid AccountId { get; init; }
    public required int PaymentCount { get; init; }
}
