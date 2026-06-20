namespace EShop.Order.Application.Models;

public sealed class OrderDto
{
    public required Guid Id { get; init; }
    public required string BuyerId { get; init; }
    public required DateTimeOffset OrderDate { get; init; }
    public required string Status { get; init; }
    public string? Description { get; init; }
    public required List<OrderItemDto> OrderItems { get; init; }

    public static OrderDto MapFrom(Domain.Aggregates.Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            BuyerId = order.BuyerId,
            OrderDate = order.OrderDate,
            Status = order.Status,
            Description = order.Description,
            OrderItems = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                VariantId = oi.VariantId,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                Discount = oi.Discount
            }).ToList()
        };
    }
}

public sealed class OrderItemDto
{
    public required Guid Id { get; init; }
    public required Guid VariantId { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
    public decimal? Discount { get; init; }
}
