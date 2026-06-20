namespace EShop.Inventory.API.Models;

public sealed class WarnUpStockAvailableRequest
{
    public string TenantId { get; set; } = string.Empty;
    public Guid VariantId { get; set; }
}
