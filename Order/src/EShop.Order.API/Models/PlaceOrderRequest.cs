using EShop.Order.Domain.Commands;

namespace EShop.Order.API.Models;

public sealed class PlaceOrderRequest
{
    public required string BuyerId { get; init; }

    public List<OrderItemData> OrderItems { get; init; }
}
