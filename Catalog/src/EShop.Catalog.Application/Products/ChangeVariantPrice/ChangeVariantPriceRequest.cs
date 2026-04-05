namespace EShop.Catalog.Application.Products.ChangeVariantPrice;

public sealed class ChangeVariantPriceRequest
{
    public required decimal Price { get; init; }
    public required decimal DiscountPrice { get; init; }
}
