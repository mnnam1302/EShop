using EShop.Shared.Contracts.Abstractions.Mediator;

namespace EShop.Order.Domain.Commands;

public sealed class PlaceOrderCommand : ICommand<Guid>
{
    public required string BuyerId { get; init; }
    public required List<OrderItemData> OrderItems { get; init; }
}

public sealed class OrderItemData
{
    public required Guid VariantId { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
    public decimal? Discount { get; init; }
}
