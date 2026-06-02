namespace EShop.Shared.Contracts.Services.Order;

/// <summary>
/// Published by the Place-Order saga when the order cannot be fulfilled
/// (insufficient stock, timeout, or compensation failure).
/// </summary>
public sealed class OrderRejected : OrderIntegrationEvent
{
    public required Guid OrderId { get; init; }

    public required string BuyerId { get; init; }

    public required string Reason { get; init; }

    public required DateTimeOffset RejectedAt { get; init; }
}
