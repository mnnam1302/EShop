namespace EShop.Shared.Contracts.Services.Catalog;

public sealed class VariantUpdated : CatalogIntegrationEvent
{
    public Guid ProductId { get; init; }
    public Guid VariantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
}
