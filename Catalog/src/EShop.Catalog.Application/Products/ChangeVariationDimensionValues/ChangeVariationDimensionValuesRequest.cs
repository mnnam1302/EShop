namespace EShop.Catalog.Application.Products.ChangeVariationDimensionValues;

public sealed class ChangeVariationDimensionValuesRequest
{
    public required string[] Values { get; init; } = [];
}
