using EShop.Shared.Scoping.ResourceAccessControl.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;

namespace EShop.Authorization.Application.Services;

internal sealed class OwnerUserPermissionProvider : IUserPermissionsProvider
{
    /// <summary>Maximum time the distributed lock is held while the cache entry is being rebuilt.</summary>
    private static readonly TimeSpan LockExpiration = TimeSpan.FromSeconds(15);

    /// <summary>Time to wait before re-reading the cache when another instance holds the rebuild lock.</summary>
    private static readonly TimeSpan LockRetryDelay = TimeSpan.FromMilliseconds(200);

    private readonly IPermissionCachingService _permissionCache;
    private readonly IPermissionCalculator _permissionCalculator;
    private readonly IDistributedLock _distributedLock;

    public OwnerUserPermissionProvider(
        IPermissionCachingService permissionCache,
        IPermissionCalculator permissionCalculator,
        IDistributedLock distributedLock)
    {
        _permissionCache = permissionCache;
        _permissionCalculator = permissionCalculator;
        _distributedLock = distributedLock;
    }

    public async Task<string[]> GetPermissions(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User Id is required", nameof(userId));
        }

        // Fast path — cache is warm.
        var cached = await _permissionCache.GetPermissionsAsync(userId);
        if (cached.Length != 0)
        {
            return cached;
        }

        // Cache miss — acquire a per-user distributed lock so only one instance recalculates
        // permissions while the others wait (prevents cache stampede).
        using var lockHandle = await _distributedLock.TryAcquireAsync(
            $"permissions:{userId}",
            LockExpiration,
            cancellationToken);

        if (lockHandle is not null)
        {
            // Double-check: another instance may have populated the cache while we waited.
            cached = await _permissionCache.GetPermissionsAsync(userId);
            if (cached.Length != 0)
            {
                return cached;
            }

            // We are the designated rebuilder — calculate and cache permissions.
            var permissions = await _permissionCalculator.CalculateFor(userId.ToLowerInvariant(), cancellationToken);
            if (permissions.Length > 0)
            {
                await _permissionCache.AddPermissionsAsync(userId, permissions);
            }

            return permissions;
        }

        // Lock busy — another instance is already rebuilding. Wait briefly, then serve
        // whatever is now in the cache (graceful degradation).
        await Task.Delay(LockRetryDelay, cancellationToken);
        return await _permissionCache.GetPermissionsAsync(userId);
    }
}
