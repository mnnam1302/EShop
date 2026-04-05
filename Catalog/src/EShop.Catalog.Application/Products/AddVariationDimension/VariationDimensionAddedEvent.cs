namespace EShop.Catalog.Application.Products.AddVariationDimension;

public sealed class VariationDimensionAddedEvent : ProductDomainEvent
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string[] Values { get; set; } = [];
    public string DisplayStyle { get; set; } = string.Empty;
}
