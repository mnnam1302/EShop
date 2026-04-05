namespace EShop.Catalog.Application.Products.UpdateVariant;

public sealed class VariantUpdatedEvent : ProductDomainEvent
{
    public required Guid VariantId { get; init; }
    public required string Name { get; init; } = string.Empty;
    public required string Sku { get; init; } = string.Empty;
    public required DateTimeOffset UpdatedAtUtc { get; init; }
    public required string UpdatedByUserId { get; init; } = string.Empty;
}
