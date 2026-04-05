using EShop.Catalog.Application.Shared;

namespace EShop.Catalog.Application.Products;

public abstract class ProductDomainEvent : ICatalogDomainEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTimeOffset TimeStampUtc { get; set; } = DateTimeOffset.UtcNow;
    public required Guid ProductId { get; init; }
    public ulong Version { get; set; }
}