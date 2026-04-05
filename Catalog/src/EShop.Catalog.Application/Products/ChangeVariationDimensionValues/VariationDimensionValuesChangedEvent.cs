namespace EShop.Catalog.Application.Products.ChangeVariationDimensionValues;

public sealed class VariationDimensionValuesChangedEvent : ProductDomainEvent
{
    public required string DimensionName { get; init; }
    public required string[] Values { get; init; } = [];
}
