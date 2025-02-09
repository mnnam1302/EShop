using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;

namespace EShop.Identity.Application.Services;

public class OwnerCacheUserPermissionService : IUserPermissionsProvider
{
    private readonly IPermissionCachingOwnerService _permissionCache;
    private readonly IPermissionCalculator _permissionCalculator;

    public OwnerCacheUserPermissionService(IPermissionCachingOwnerService permissionCache, IPermissionCalculator permissionCalculator)
    {
        _permissionCache = permissionCache;
        _permissionCalculator = permissionCalculator;
    }

    public async Task<string[]> GetPermissions(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User Id is required", nameof(userId));
        }

        if (_permissionCache.TryGetPermissions(userId, out var userPermissionsFromCache))
        {
            return userPermissionsFromCache;
        }

        var calculatedPermissions = await _permissionCalculator.CalculateFor(userId.ToLower());

        _permissionCache.AddPermissions(userId, calculatedPermissions);

        return calculatedPermissions;
    }
}