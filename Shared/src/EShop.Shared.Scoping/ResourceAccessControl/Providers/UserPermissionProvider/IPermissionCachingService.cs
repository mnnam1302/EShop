namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;

public interface IPermissionCachingService
{
    Task<string[]> GetPermissionsAsync(string userId);

    Task AddPermissionsAsync(string userId, string[] permissions);

    Task RemoveCacheAsync(string userId);
}