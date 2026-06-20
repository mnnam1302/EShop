using EShop.Shared.Contracts.Abstractions.Mediator;

namespace EShop.Inventory.Domain.Commands;

public sealed class ReserveStocksCommand : ICommand
{
    public required Guid OrderId { get; init; }
    public required IReadOnlyList<OrderItem> Items { get; init; }
    public required string TenantId { get; init; }
    public required string ActionUserId { get; init; }
    public required string ActionUserType { get; init; }
}

public sealed class OrderItem
{
    public required Guid VariantId { get; init; }
    public required int Quantity { get; init; }
}
