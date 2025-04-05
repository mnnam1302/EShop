namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;

/// <summary>
/// Represents the user permissions provider.
/// This permissions provider attempts to update the remote cache. Thus, it should be
/// used by services that are not the owner of the permissions cache entries in the remote cache.
/// </summary>
public class UserPermissionProvider : IUserPermissionsProvider
{
    private readonly IPermissionCachingService _permissionCache;
    private readonly UserPermissionHttpClient _userPermissionHttpClient;

    public UserPermissionProvider(IPermissionCachingService permissionCache, UserPermissionHttpClient userPermissionHttpClient)
    {
        _permissionCache = permissionCache;
        _userPermissionHttpClient = userPermissionHttpClient;
    }
    public async Task<string[]> GetPermissions(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User Id is required", nameof(userId));
        }

        var permissionsCache = await _permissionCache.GetPermissionsAsync(userId);
        if (permissionsCache.Any())
        {
            return permissionsCache;
        }

        var permissions = await _userPermissionHttpClient.GetPermissionsForCurrentUser();

        return permissions;
    }
}