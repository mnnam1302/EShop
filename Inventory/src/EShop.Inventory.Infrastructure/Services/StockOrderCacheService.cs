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

        logger.LogInformation("Stock available normal: key: '{Key}', value:'{Value}', newStock:'{NewStockAvailable}'",
            stockCacheKey,
            oldStockAvailable,
            oldStockAvailable - quantity);

        // 2. Decrease stock
        var newStockAvailable = oldStockAvailable - quantity; // 100 - 1 = 99
        await _redisDatabase.StringSetAsync(stockCacheKey, newStockAvailable); // 99
        logger.LogInformation("Stock available racing...: newStock:'{NewStockAvailable}'", newStockAvailable);

        return oldStockAvailable;
    }

    private static readonly LuaScript _decreaseScript = LuaScript.Prepare(
        """
        local stock = redis.call('GET', @stockKey)
        if not stock then return -1 end
        stock = tonumber(stock)
        local qty = tonumber(@quantity)
        if stock >= qty then
            redis.call('DECRBY', @stockKey, qty)
            return 1
        else
            return 0
        end
        """);

    /// <summary>
    /// Evaluates stock via an atomic LUA script to prevent race conditions (Overselling).
    /// Returns standardized status codes: 1 (Success), 0 (Out of Stock), -1 (Cache Miss).
    /// </summary>
    public async Task<int> DecreaseStockCacheByLUA(Guid variantId, int quantity)
    {
        var stockCacheKey = InventoryCacheKeyProvider.GetStockItemCacheKey(variantId.ToString());

        var result = (int)await _redisDatabase.ScriptEvaluateAsync(
            _decreaseScript,
            new { stockKey = (RedisKey)stockCacheKey, quantity = (RedisValue)quantity });

        logger.LogInformation("LUA Stock Deduction Result for Key '{Key}': {StatusCode}", stockCacheKey, result);

        return result;
    }

    private static readonly LuaScript _increaseScript = LuaScript.Prepare(
        """
        local exists = redis.call('EXISTS', @stockKey)
        if exists == 0 then return 0 end
        redis.call('INCRBY', @stockKey, @quantity)
        return 1
        """);

    /// <summary>
    /// Reverts/Refunds stock back to Redis asynchronously.
    /// Uses the atomic INCRBY command to prevent concurrent data pollution during rollbacks.
    /// </summary>
    public async Task<bool> IncreaseStockCache(Guid variantId, int quantity)
    {
        var stockCacheKey = InventoryCacheKeyProvider.GetStockItemCacheKey(variantId.ToString());

        var result = (int)await _redisDatabase.ScriptEvaluateAsync(
            _increaseScript,
            new { stockKey = (RedisKey)stockCacheKey, quantity = (RedisValue)quantity });

        if (result == 0)
        {
            logger.LogWarning("RollbackCache: Key for variant '{VariantId}' not found — rollback skipped, DB is source of truth", variantId);
        }

        return result == 1;
    }
}
