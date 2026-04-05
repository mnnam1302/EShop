namespace EShop.Catalog.Application.Products.UpdateVariationDimension;

public sealed class VariationDimensionUpdatedEvent : ProductDomainEvent
{
    public string Name { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string DisplayStyle { get; init; } = string.Empty;
}
