using EShop.Catalog.Application.Shared;

namespace EShop.Catalog.SyncService.Abstractions;

public interface IReadModelSyncEventHandler
{
    Task HandleAsync(ICatalogDomainEvent @event);
}