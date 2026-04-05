namespace EShop.Catalog.Application.Products.Unpublish;

public sealed class ProductUnpublishedEvent : ProductDomainEvent
{
    public required DateTimeOffset UnpublishedAtUtc { get; init; }
    public required string UnpublishedByUserId { get; init; }
}