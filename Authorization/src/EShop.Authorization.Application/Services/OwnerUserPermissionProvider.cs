using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;

namespace EShop.Authorization.Application.Services;

internal sealed class OwnerUserPermissionProvider : IUserPermissionsProvider
{
    private readonly IPermissionCachingService _permissionCache;
    private readonly IPermissionCalculator _permissionCalculator;

    public OwnerUserPermissionProvider(IPermissionCachingService permissionCache, IPermissionCalculator permissionCalculator)
    {
        _permissionCache = permissionCache;
        _permissionCalculator = permissionCalculator;
    }

    public async Task<string[]> GetPermissions(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User Id is required", nameof(userId));
        }

        var permissionsCache = await _permissionCache.GetPermissionsAsync(userId);
        if (permissionsCache.Length != 0)
        {
            return permissionsCache;
        }

        var calculatedPermissions = await _permissionCalculator.CalculateFor(userId.ToLower());

        await _permissionCache.AddPermissionsAsync(userId, calculatedPermissions);

        return calculatedPermissions;
    }
}
