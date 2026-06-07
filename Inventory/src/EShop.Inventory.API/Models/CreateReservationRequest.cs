using EShop.Inventory.Domain.Commands;

namespace EShop.Inventory.API.Models;

public sealed class CreateReservationRequest
{
    public Guid? OrderId { get; init; }
    public IReadOnlyList<OrderItem> Items { get; init; } = [];
}