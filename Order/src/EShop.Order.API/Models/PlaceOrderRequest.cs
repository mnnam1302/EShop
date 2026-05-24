using EShop.Order.Application.UseCases.Orders;
using EShop.Order.Domain.Commands;

namespace EShop.Order.API.Models;

public sealed class PlaceOrderRequest
{
    public string BuyerId { get; init; }

    public List<OrderItemData> OrderItems { get; init; }
}
