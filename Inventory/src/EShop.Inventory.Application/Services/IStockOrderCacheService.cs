namespace EShop.Inventory.Application.Services;

public interface IStockOrderCacheService
{
    Task AddStockAvailable(Guid variantId, int stockAvailable);

    Task<int> DecreaseStockCache(Guid variantId, int quantity);

    Task<int> DecreaseStockCacheByLUA(Guid variantId, int quantity);
}
