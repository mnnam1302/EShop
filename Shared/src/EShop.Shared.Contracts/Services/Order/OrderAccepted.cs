namespace EShop.Shared.Contracts.Services.Order;

/// <summary>
/// Published by the Place-Order saga once the order has been fully accepted
/// (stock reserved + order row persisted). Consumed by downstream services
/// such as Notification and Finance (future).
/// </summary>
public sealed class OrderAccepted : OrderIntegrationEvent
{
    public required Guid OrderId { get; init; }

    public required string BuyerId { get; init; }

    public required DateTimeOffset AcceptedAt { get; init; }
}
