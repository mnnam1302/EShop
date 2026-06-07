using EShop.Inventory.Application.Services;
using EShop.Shared.Cache.CacheKeys;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace EShop.Inventory.Infrastructure.Services;

public sealed class StockOrderCacheService(
    ILogger<StockOrderCacheService> logger,
    IConnectionMultiplexer connectionMultiplexer) : IStockOrderCacheService
{
    private readonly IDatabase _redisDatabase = connectionMultiplexer.GetDatabase();

    public async Task AddStockAvailable(Guid variantId, int stockAvailable)
    {
        var stockCacheKey = InventoryCacheKeyProvider.GetStockItemCacheKey(variantId.ToString());
        await _redisDatabase.StringSetAsync(stockCacheKey, stockAvailable);

        logger.LogInformation("Warned up stock available for variant '{VariantId}' with KEY: {Key} and VALUE: {Value}",
            variantId, stockCacheKey, stockAvailable);
    }

    public async Task<int> DecreaseStockCache(Guid variantId, int quantity)
    {
        var stockCacheKey = InventoryCacheKeyProvider.GetStockItemCacheKey(variantId.ToString());

        // 1. Get stock available
        var stockCacheValue = await _redisDatabase.StringGetAsync(stockCacheKey);
        if (!stockCacheValue.HasValue)
        {
            logger.LogWarning("Stock cache entry not found for key: {StockCacheKey}", stockCacheKey);
            return 0;
        }

        int oldStockAvailable = (int)stockCacheValue;
        if (oldStockAvailable < quantity)
        {
            return 0;
        }

        logger.LogInformation("Stock available normal: {Key}, {Value}, {NewStockAvailable}",
            stockCacheValue,
            oldStockAvailable,
            oldStockAvailable - quantity);

        // 2. Decrease stock
        var newStockAvailable = oldStockAvailable - quantity; // 100 - 1 = 99
        await _redisDatabase.StringSetAsync(stockCacheKey, newStockAvailable); // 99
        logger.LogInformation("Stock available racing...: {NewStockAvailable}", newStockAvailable);

        return oldStockAvailable;
    }

    public Task DecreaseStockCacheByLUA(Guid variantId, int quantity)
    {
        throw new NotImplementedException();
    }
}
