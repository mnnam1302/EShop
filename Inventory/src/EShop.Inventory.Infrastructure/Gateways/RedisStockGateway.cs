using EShop.Inventory.Domain.Abstractions;
using StackExchange.Redis;

namespace EShop.Inventory.Infrastructure.Gateways;

/// <summary>
/// Atomic Redis-backed stock reservation gateway using Lua scripts.
///
/// Key schema:
///   stock:available:{variantId}  — available units (integer)
///   stock:reserved:{variantId}   — reserved units (integer)
///   stock:_initialized           — sentinel; presence means Redis is seeded
///
/// Lua guarantees single-threaded execution per Redis node, making the
/// check-and-reserve fully atomic without distributed locking overhead.
/// </summary>
internal sealed class RedisStockGateway : IRedisStockGateway
{
    private readonly IConnectionMultiplexer _redis;

    // Lua: Phase 1 — check; Phase 2 — reserve (all-or-nothing).
    // KEYS[i*2-1] = available key, KEYS[i*2] = reserved key for item i
    // ARGV[i]     = requested quantity for item i
    // Returns 1 if all reserved, 0 if any item has insufficient stock.
    private static readonly string _reserveLuaScript = """
        local count = #KEYS / 2
        -- Phase 1: check all
        for i = 1, count do
            local avail = tonumber(redis.call('GET', KEYS[i*2-1])) or 0
            local qty   = tonumber(ARGV[i]) or 0
            if avail < qty then
                return 0
            end
        end
        -- Phase 2: reserve all
        for i = 1, count do
            local qty = tonumber(ARGV[i]) or 0
            redis.call('DECRBY', KEYS[i*2-1], qty)
            redis.call('INCRBY', KEYS[i*2],   qty)
        end
        return 1
        """;

    // Lua: release reserved units back to available.
    // KEYS[i*2-1] = available key, KEYS[i*2] = reserved key
    // ARGV[i]     = quantity to release
    private static readonly string _releaseLuaScript = """
        local count = #KEYS / 2
        for i = 1, count do
            local qty = tonumber(ARGV[i]) or 0
            redis.call('INCRBY', KEYS[i*2-1], qty)
            redis.call('DECRBY', KEYS[i*2],   qty)
        end
        return 1
        """;

    private const string _sentinelKey = "stock:_initialized";

    public RedisStockGateway(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<bool> TryReserveAsync(
        IReadOnlyList<StockReservationRequest> items,
        CancellationToken cancellationToken = default)
    {
        if (items.Count == 0)
        {
            return true;
        }

        var db = _redis.GetDatabase();

        var keys = BuildKeys(items);
        var args = BuildArgs(items);

        var result = (long)await db.ScriptEvaluateAsync(
            _reserveLuaScript,
            keys,
            args);

        return result == 1;
    }

    public async Task ReleaseAsync(
        IReadOnlyList<StockReservationRequest> items,
        CancellationToken cancellationToken = default)
    {
        if (items.Count == 0)
        {
            return;
        }

        var db = _redis.GetDatabase();

        await db.ScriptEvaluateAsync(
            _releaseLuaScript,
            BuildKeys(items),
            BuildArgs(items));
    }

    public async Task SeedStockAsync(Guid variantId, int availableStock, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        await db.StringSetAsync(AvailableKey(variantId), availableStock);
    }

    public async Task<bool> IsInitializedAsync(CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        return await db.KeyExistsAsync(_sentinelKey);
    }

    // ── internal helpers ──────────────────────────────────────────────────────

    private static RedisKey[] BuildKeys(IReadOnlyList<StockReservationRequest> items)
    {
        var keys = new RedisKey[items.Count * 2];
        for (var i = 0; i < items.Count; i++)
        {
            keys[i * 2] = AvailableKey(items[i].VariantId);
            keys[i * 2 + 1] = ReservedKey(items[i].VariantId);
        }

        return keys;
    }

    private static RedisValue[] BuildArgs(IReadOnlyList<StockReservationRequest> items)
    {
        var args = new RedisValue[items.Count];
        for (var i = 0; i < items.Count; i++)
        {
            args[i] = items[i].Quantity;
        }

        return args;
    }

    private static string AvailableKey(Guid variantId) => $"stock:available:{variantId}";
    private static string ReservedKey(Guid variantId) => $"stock:reserved:{variantId}";
}
