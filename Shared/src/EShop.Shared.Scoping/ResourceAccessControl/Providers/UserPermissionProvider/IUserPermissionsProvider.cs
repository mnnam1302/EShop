namespace EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;

public interface IUserPermissionsProvider
{
    Task<string[]> GetPermissions(string userId);
}