using EShop.Shared.Scoping.ResourceAccessControl.Providers;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace EShop.Shared.Cache.Services;

internal sealed class RedisDistributedLock : IDistributedLock
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisDistributedLock> _logger;

    public RedisDistributedLock(IConnectionMultiplexer redis, ILogger<RedisDistributedLock> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<IDisposable?> TryAcquireAsync(string resource, TimeSpan expirationTime, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var lockKey = $"lock:{resource}";
        var lockValue = Guid.NewGuid().ToString();

        // Try to acquire the lock using SET NX with expiration
        var acquired = await db.StringSetAsync(
            lockKey,
            lockValue,
            expirationTime,
            When.NotExists);

        if (!acquired)
        {
            _logger.LogDebug("Failed to acquire lock for resource: {Resource}", resource);
            return null;
        }

        _logger.LogDebug("Lock acquired for resource: {Resource}", resource);

        return new RedisLockHandle(db, lockKey, lockValue, _logger);
    }

    /// <summary>
    /// Lock Key Uniqueness: Use hierarchical, descriptive keys (order:12345, inventory:product-456) to avoid collisions.
    /// Lock Value: We use a single distinct GUID as the lock value. This ensures only the lock owner can release it,
    /// excluding unintentional deletion by expired locks or other operations.
    ///
    /// Automatic Expiration: Always provide an expiration time to prevent deadlocks when a process halts with an outstanding lock.
    ///
    /// LUA Script for Release: Releasing uses a LUA script to atomically check ownership and delete the key.
    /// This prevents releasing a lock that has already timed out and is reacquired by another process.
    ///
    /// Disposal Pattern: With IDisposable and await using, one ensures that the lock is released regardless of the exception that occurs.
    /// </summary>
    private sealed class RedisLockHandle : IDisposable
    {
        private readonly IDatabase _db;
        private readonly string _lockKey;
        private readonly string _lockValue;
        private readonly ILogger _logger;
        private bool _disposed;

        public RedisLockHandle(IDatabase db, string lockKey, string lockValue, ILogger logger)
        {
            _db = db;
            _lockKey = lockKey;
            _lockValue = lockValue;
            _logger = logger;
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                // Use a Lua script to release the lock only if the value matches
                var script = @"
                if redis.call('get', KEYS[1]) == ARGV[1] then
                    return redis.call('del', KEYS[1])
                else
                    return 0
                end";

                _db.ScriptEvaluate(
                    script,
                    new RedisKey[] { _lockKey },
                    new RedisValue[] { _lockValue });

                _logger.LogDebug("Lock released for key: {LockKey}", _lockKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing lock for key: {LockKey}", _lockKey);
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}
