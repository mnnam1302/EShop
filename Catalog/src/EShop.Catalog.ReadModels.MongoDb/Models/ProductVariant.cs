namespace EShop.Catalog.ReadModels.MongoDb.Models;

public sealed class ProductVariant
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal DiscountPrice { get; set; }
    public bool IsDefault { get; set; }
    public string State { get; set; } = string.Empty;
    public List<ProductVariantDimensionValue> DimensionValues { get; set; } = [];
}
