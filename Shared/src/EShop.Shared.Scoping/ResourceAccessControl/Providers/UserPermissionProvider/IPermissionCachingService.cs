namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;

public interface IPermissionCachingService
{
    bool TryGetPermissions(string userId, out string[] permissions);

    void AddPermissions(string userId, string[] permissions);

    void RemoveCache(string userId);
}