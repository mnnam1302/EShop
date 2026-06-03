using EShop.Shared.Contracts.Abstractions.Mediator;
using EShop.Shared.Contracts.Services.Order;

namespace EShop.Inventory.Domain.Commands;

public sealed class MakeReservationsCommand : ICommand
{
    public required Guid OrderId { get; init; }
    public required IReadOnlyList<OrderItem> Items { get; init; }
}
