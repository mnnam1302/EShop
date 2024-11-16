using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;

namespace EShop.Identity.Application.Services;

public class OwnerCacheUserPermissionService : IUserPermissionsProvider
{
    private readonly IPermissionCachingOwnerService permissionCache;
    private readonly IPermissionCalculator permissionCalculator;

    public OwnerCacheUserPermissionService(IPermissionCachingOwnerService permissionCache, IPermissionCalculator permissionCalculator)
    {
        this.permissionCache = permissionCache;
        this.permissionCalculator = permissionCalculator;
    }

    public async Task<string[]> GetPermissions(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User Id is required", nameof(userId));
        }

        if (permissionCache.TryGetPermissions(userId, out var userPermissionsFromCache))
        {
            return userPermissionsFromCache;
        }

        var calculatedPermissions = await this.permissionCalculator.CalculateFor(userId.ToLower());

        permissionCache.AddPermissions(userId, calculatedPermissions);

        return calculatedPermissions;
    }
}