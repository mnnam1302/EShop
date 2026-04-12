namespace EShop.Catalog.Application.Products.Delete;

public sealed class ProductDeletedEvent : ProductDomainEvent
{
    public required DateTimeOffset DeletedAtUtc { get; init; }
    public required string DeletedByUserId { get; init; }
}