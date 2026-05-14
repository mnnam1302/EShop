namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;

/// <summary>
/// Represents the user permissions provider.
/// This permissions provider attempts to update the remote cache. Thus, it should be
/// used by services that are not the owner of the permissions cache entries in the remote cache.
/// </summary>
public sealed class UserPermissionProvider : IUserPermissionsProvider
{
    /// <summary>Maximum time the distributed lock is held while the cache entry is being rebuilt.</summary>
    private static readonly TimeSpan LockExpiration = TimeSpan.FromSeconds(15);

    /// <summary>Time to wait before re-reading the cache when another instance holds the rebuild lock.</summary>
    private static readonly TimeSpan LockRetryDelay = TimeSpan.FromMilliseconds(200);

    private readonly IPermissionCachingService _permissionCache;
    private readonly UserPermisssionHttpClient _userPermissionHttpClient;
    private readonly IDistributedLock _distributedLock;

    public UserPermissionProvider(
        IPermissionCachingService permissionCache,
        UserPermisssionHttpClient userPermissionHttpClient,
        IDistributedLock distributedLock)
    {
        _permissionCache = permissionCache;
        _userPermissionHttpClient = userPermissionHttpClient;
        _distributedLock = distributedLock;
    }

    public async Task<string[]> GetPermissions(string userId, CancellationToken cancellationToken = default)
    {
        // 1. Fast path — cache is warm
        var cached = await _permissionCache.GetPermissionsAsync(userId);
        if (cached.Length != 0)
        {
            return cached;
        }

        // 2. Slow path — acquire per-user lock
        using var lockHandle = await _distributedLock.TryAcquireAsync(
            $"permissions:{userId}",
            LockExpiration,
            cancellationToken);

        if (lockHandle is not null)
        {
            // 3. Double-check: another instance may have just populated the cache
            cached = await _permissionCache.GetPermissionsAsync(userId);
            if (cached.Length != 0)
            {
                return cached;
            }

            // 4. We are the designated rebuilder
            var permissions = await _userPermissionHttpClient.GetPermissionsForCurrentUser();
            if (permissions.Length > 0)
            {
                await _permissionCache.AddPermissionsAsync(userId, permissions);
            }

            return permissions;
        }

        // 5. Lock busy — another instance is rebuilding; wait and serve from cache
        await Task.Delay(LockRetryDelay, cancellationToken);
        return await _permissionCache.GetPermissionsAsync(userId);
    }
}
