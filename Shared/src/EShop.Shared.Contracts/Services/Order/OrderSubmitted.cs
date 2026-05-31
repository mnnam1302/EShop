namespace EShop.Shared.Contracts.Services.Order;

public sealed class OrderSubmitted : OrderIntegrationEvent
{
    public required Guid OrderId { get; init; }

    public required string BuyerId { get; init; }

    public required IReadOnlyList<OrderItem> Items { get; init; }

    public required DateTimeOffset SubmittedAt { get; init; }
}

public sealed class OrderItem
{
    public required Guid VariantId { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
    public decimal? Discount { get; init; }
}
