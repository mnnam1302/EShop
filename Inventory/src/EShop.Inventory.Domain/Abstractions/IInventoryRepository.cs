using EShop.Shared.DomainTools.Repositories;

namespace EShop.Inventory.Domain.Abstractions;

public interface IInventoryRepository : IRepositoryBase<Aggregates.Inventory, Guid>
{
    Task DecreaseStockLevel1(Guid variantId, int quantity, CancellationToken cancellationToken);

    Task DecreaseStockLevel3CAS(Guid variantId, int oldStockAvailable, int quantity, CancellationToken cancellationToken);
}
