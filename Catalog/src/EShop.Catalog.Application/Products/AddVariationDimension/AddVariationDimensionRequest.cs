namespace EShop.Catalog.Application.Products.AddVariationDimension;

public sealed class AddVariationDimensionRequest
{
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public required string[] Values { get; init; } = [];
    public required string DisplayStyle { get; init; }
}
