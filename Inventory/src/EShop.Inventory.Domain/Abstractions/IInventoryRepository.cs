using EShop.Shared.DomainTools.Repositories;

namespace EShop.Inventory.Domain.Abstractions;

public interface IInventoryRepository : IRepositoryBase<Aggregates.Inventory, Guid>
{
    /// <summary>
    /// Atomically deducts <paramref name="quantity"/> from stock_available
    /// using a conditional UPDATE (WHERE stock_available >= quantity).
    /// Returns 1 if deducted, 0 if insufficient stock.
    /// </summary>
    Task<int> DeductStocLevel1Async(Guid variantId, string tenantId, int quantity, CancellationToken cancellationToken);

    /// <summary>
    /// Adds <paramref name="quantity"/> back to stock_available (release/expiry path).
    /// </summary>
    Task AddBackStockAsync(Guid variantId, string tenantId, int quantity, CancellationToken cancellationToken);
}
