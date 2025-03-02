using EShop.Shared.Cache.CacheKeys;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace EShop.Shared.Cache.Services;

public class PermissionRedisCachingService : IPermissionCachingService
{
    private readonly IRedisCachingAsyncProvider<string[]> _redisCachingAsyncProvider;
    private readonly ILogger _logger;
    private readonly CachedRemoteConfiguration _cachedRemoteConfiguration;

    public PermissionRedisCachingService(
        IRedisCachingAsyncProvider<string[]> redisCachingAsyncProvider,
        ILogger<PermissionRedisCachingService> logger,
        CachedRemoteConfiguration cachedRemoteConfiguration)
    {
        _redisCachingAsyncProvider = redisCachingAsyncProvider;
        _logger = logger;
        _cachedRemoteConfiguration = cachedRemoteConfiguration;
    }

    public async Task AddPermissionsAsync(string userId, string[] permissions)
    {
        await _redisCachingAsyncProvider.AddAsync(
            UserPermissionCacheKeyProvider.GetCacheKey(userId),
            permissions,
            new DistributedCacheEntryOptions { SlidingExpiration = _cachedRemoteConfiguration.GetSlidingExpiration() });
    }

    public async Task RemoveCacheAsync(string userId)

    {
        await _redisCachingAsyncProvider.ClearAsync(UserPermissionCacheKeyProvider.GetCacheKey(userId));
    }

    public async Task<string[]> GetPermissionsAsync(string userId)
    {
        var permissions = Array.Empty<string>();
        try
        {
            var cachedPermission = await _redisCachingAsyncProvider.GetAsync(UserPermissionCacheKeyProvider.GetCacheKey(userId));
            permissions = cachedPermission ?? permissions;

            return permissions;
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis connection exception '{FailureType}' while retrieving cached permission for user '{UserId}'", ex.FailureType, userId);
            return permissions;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception while retrieving cached permission for user '{UserId}'", userId);
            return permissions;
        }
    }
}