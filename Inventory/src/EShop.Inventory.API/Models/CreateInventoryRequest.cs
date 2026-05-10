namespace EShop.Inventory.API.Models;

public sealed class CreateInventoryRequest
{
    public required Guid ProductId { get; init; }
    public required Guid VariantId { get; init; }
    public required string Sku { get; init; }
    public required int StockAvailable { get; init; }
    public int MinimumStock { get; init; }
}
