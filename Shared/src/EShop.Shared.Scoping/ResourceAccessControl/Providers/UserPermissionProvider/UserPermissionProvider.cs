namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;

/// <summary>
/// Represents the user permissions provider.
/// This permissions provider attempts to update the remote cache. Thus, it should be
/// used by services that are not the owner of the permissions cache entries in the remote cache.
/// </summary>
public class UserPermissionProvider : IUserPermissionsProvider
{
    private readonly IPermissionCachingService _permissionCache;
    private readonly UserPermisssionHttpClient _userPermissionHttpClient;

    public UserPermissionProvider(IPermissionCachingService permissionCache, UserPermisssionHttpClient userPermissionHttpClient)
    {
        _permissionCache = permissionCache;
        _userPermissionHttpClient = userPermissionHttpClient;
    }
    public async Task<string[]> GetPermissions(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentNullException(nameof(userId), "User ID is required");
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