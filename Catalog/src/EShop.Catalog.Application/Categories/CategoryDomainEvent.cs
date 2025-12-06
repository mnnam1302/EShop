using EShop.Catalog.Application.Shared;
using EShop.Shared.Contracts.Abstractions.MessageBus;

namespace EShop.Catalog.Application.Categories;

public abstract class CategoryDomainEvent : ICatalogDomainEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public DateTimeOffset TimeStampUtc { get; set; } = DateTimeOffset.UtcNow;
    public Guid CategoryId { get; set; }
    public ulong Version { get; set; }
}
