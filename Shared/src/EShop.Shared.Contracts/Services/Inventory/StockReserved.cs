namespace EShop.Shared.Contracts.Services.Inventory;

public sealed class StockReserved : InventoryIntegrationEvent
{
    public required Guid OrderId { get; init; }
    public required Guid ReservationId { get; init; }
}
