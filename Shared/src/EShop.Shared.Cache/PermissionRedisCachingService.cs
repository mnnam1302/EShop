using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace EShop.Shared.Cache;

public class PermissionRedisCachingService : IPermissionCachingOwnerService, IPermissionCachingService
{
    private readonly IRedisCachingProvider<string[]> redisCachingProvider;
    private readonly ILogger<PermissionRedisCachingService> logger;

    public PermissionRedisCachingService(
        IRedisCachingProvider<string[]> redisCachingProvider,
        ILogger<PermissionRedisCachingService> logger)
    {
        this.redisCachingProvider = redisCachingProvider;
        this.logger = logger;
    }

    public void AddPermissions(string userId, string[] permissions)
    {
        redisCachingProvider.AddValue(UserPermissionCacheKeyProvider.GetCacheKey(userId), permissions);
    }

    public void RemoveCache(string userId)
    {
        redisCachingProvider.ClearCache(UserPermissionCacheKeyProvider.GetCacheKey(userId));
    }

    public bool TryGetPermissions(string userId, out string[] permissions)
    {
        permissions = Array.Empty<string>();
        try
        {
            var cachedPermission = redisCachingProvider.GetValue(UserPermissionCacheKeyProvider.GetCacheKey(userId));
            permissions = cachedPermission ?? permissions;
            return cachedPermission != null && cachedPermission.Length > 0;
        }
        catch (RedisConnectionException ex)
        {
            //logger.LogWarning(LogEvents.RedisConnectionError, ex, "Redis connection exception '{FailureType}' while retrieving cached permission for user '{userId}'", ex.FailureType, userId);
            logger.LogWarning(ex, "Redis connection exception '{FailureType}' while retrieving cached permission for user '{userId}'", ex.FailureType, userId);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception while retrieving cached permission for user '{userId}'", userId);
        }
        return false;
    }
}