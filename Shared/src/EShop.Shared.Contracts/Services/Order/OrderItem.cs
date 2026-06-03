namespace EShop.Shared.Contracts.Services.Order;

public sealed class OrderItem
{
    public required Guid VariantId { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
    public decimal? Discount { get; init; }
}
