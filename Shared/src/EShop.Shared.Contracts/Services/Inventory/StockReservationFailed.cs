namespace EShop.Shared.Contracts.Services.Inventory;

public sealed class StockReservationFailed : InventoryIntegrationEvent
{
    public required Guid OrderId { get; init; }
    public required string FailureReason { get; init; }
}
