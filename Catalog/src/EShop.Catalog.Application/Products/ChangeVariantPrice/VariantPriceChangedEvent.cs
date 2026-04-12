namespace EShop.Catalog.Application.Products.ChangeVariantPrice;

public sealed class VariantPriceChangedEvent : ProductDomainEvent
{
    public required Guid VariantId { get; init; }
    public required decimal OldPrice { get; init; }
    public required decimal NewPrice { get; init; }
    public required decimal OldDiscountPrice { get; init; }
    public required decimal NewDiscountPrice { get; init; }
    public required DateTimeOffset ChangedAtUtc { get; init; }
    public required string ChangedByUserId { get; init; }
}
