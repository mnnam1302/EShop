namespace EShop.Shared.Contracts.Services.Catalog;

public sealed class VariantPriceChanged : CatalogIntegrationEvent
{
    public Guid ProductId { get; init; }
    public Guid VariantId { get; init; }
    public decimal OldPrice { get; init; }
    public decimal NewPrice { get; init; }
    public decimal OldDiscountPrice { get; init; }
    public decimal NewDiscountPrice { get; init; }
}
