namespace EShop.Catalog.Application.Products.UpdateVariant;

public sealed class UpdateVariantRequest
{
    public required string Name { get; init; } = string.Empty;
    public required string Sku { get; init; } = string.Empty;
}
