using EShop.Inventory.Domain.Abstractions;
using EShop.Shared.Cache.CacheKeys;
using StackExchange.Redis;

namespace EShop.Inventory.Infrastructure.Gateways;

/// <summary>
/// Atomic Redis-backed stock manager using Lua Scripts.
/// Lua executes single-threaded inside Redis, ensuring atomicity without distributed locks.
/// </summary>
internal sealed class RedisStockGateway : IRedisStockGateway
{
    private readonly IConnectionMultiplexer _redis;
    private const string RedisSentinelKey = "stock:_initialized";

    public RedisStockGateway(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    // Lua Script: Atomic multi-item check and reserve (All-or-Nothing)
    // KEYS[i*2-1] = Available key, KEYS[i*2] = Reserved key
    // ARGV[i]     = Target quantity
    private const string ReserveStockLuaScript = """
        local count = #KEYS / 2
        
        -- Phase 1: Dry run validation
        for i = 1, count do
            local avail = tonumber(redis.call('GET', KEYS[i*2-1])) or 0
            local qty   = tonumber(ARGV[i]) or 0
            if avail < qty then
                return 0 -- Fail immediately if any item is out of stock
            end
        end
        
        -- Phase 2: Atomic execution
        for i = 1, count do
            local qty = tonumber(ARGV[i]) or 0
            redis.call('DECRBY', KEYS[i*2-1], qty) -- Deduct from sellable pool
            redis.call('INCRBY', KEYS[i*2],   qty) -- Hold inside holding pool
        end
        return 1
        """;

    public async Task<bool> TryReserveAsync(IReadOnlyList<StockReservationRequest> items, CancellationToken cancellationToken = default)
    {
        if (items.Count == 0) return true;

        var db = _redis.GetDatabase();

        var keys = FlattenItemKeys(items);
        var args = FlattenItemQuantities(items);

        var result = (long)await db.ScriptEvaluateAsync(ReserveStockLuaScript, keys, args);

        return result == 1;
    }


    // Lua Script: Revert temporary reservation holdings back to the sellable pool
    private const string ReleaseStockLuaScript = """
        local count = #KEYS / 2
        for i = 1, count do
            local qty = tonumber(ARGV[i]) or 0
            redis.call('INCRBY', KEYS[i*2-1], qty)
            redis.call('DECRBY', KEYS[i*2],   qty)
        end
        return 1
        """;

    public async Task ReleaseAsync(IReadOnlyList<StockReservationRequest> items, CancellationToken cancellationToken = default)
    {
        if (items.Count == 0) return;

        var db = _redis.GetDatabase();

        await db.ScriptEvaluateAsync(ReleaseStockLuaScript, FlattenItemKeys(items), FlattenItemQuantities(items));
    }

    public async Task SeedStockAsync(Guid variantId, int availableStock, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var stockAvailableKey = InventoryCacheKeyProvider.GetAvailableStockKey(variantId);
        await db.StringSetAsync(stockAvailableKey, availableStock);
    }

    public async Task<bool> IsInitializedAsync(CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        return await db.KeyExistsAsync(RedisSentinelKey);
    }

    // Maps domain array into interleaved layout: [AvailableKey_1, ReservedKey_1, AvailableKey_2...]
    private static RedisKey[] FlattenItemKeys(IReadOnlyList<StockReservationRequest> items)
    {
        var keys = new RedisKey[items.Count * 2];
        for (var i = 0; i < items.Count; i++)
        {
            keys[i * 2] = InventoryCacheKeyProvider.GetAvailableStockKey(items[i].VariantId);
            keys[i * 2 + 1] = InventoryCacheKeyProvider.GetReservedStockKey(items[i].VariantId);
        }

        return keys;
    }

    private static RedisValue[] FlattenItemQuantities(IReadOnlyList<StockReservationRequest> items)
    {
        var args = new RedisValue[items.Count];
        for (var i = 0; i < items.Count; i++)
        {
            args[i] = items[i].Quantity;
        }

        return args;
    }
}
