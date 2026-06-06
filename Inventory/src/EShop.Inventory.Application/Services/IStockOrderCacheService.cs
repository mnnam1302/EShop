namespace EShop.Inventory.Application.Services;

public interface IStockOrderCacheService
{
    Task AddStockAvailable(Guid variantId, int stockAvailable);

    Task<int> DecreaseStockCache(Guid variantId, int quantity);

    Task DecreaseStockCacheByLUA(Guid variantId, int quantity);
}
