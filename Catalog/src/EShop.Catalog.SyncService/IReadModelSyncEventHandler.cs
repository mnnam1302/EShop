using EShop.Catalog.Application.Shared;

namespace EShop.Catalog.SyncService;

public interface IReadModelSyncEventHandler
{
    Task HandleAsync(ICatalogDomainEvent @event);
}