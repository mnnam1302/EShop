namespace EShop.Catalog.ReadModels.MongoDb.Models;

public sealed class ProductVariationDimension
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string[] Values { get; set; } = [];
    public string DisplayStyle { get; set; } = string.Empty;
}
