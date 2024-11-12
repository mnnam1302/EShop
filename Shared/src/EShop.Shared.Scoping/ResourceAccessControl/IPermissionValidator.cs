using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;

namespace EShop.Shared.Scoping.ResourceAccessControl;

public interface IPermissionValidator
{
    Task<bool> HasPermissionAsync(string permissionId);
    Task<bool> HasAtLeastOneOfSpecificPermissionAsync(params string[] permissionIds);
    Task<bool> HasSupportUserAccessAsync();
}

public class AllowAllPermissionsValidator : IPermissionValidator
{
    public Task<bool> HasPermissionAsync(string permissionId)
    {
        return Task.FromResult(true);
    }

    public Task<bool> HasAtLeastOneOfSpecificPermissionAsync(params string[] permissionIds)
    {
        return Task.FromResult(true);
    }

    public Task<bool> HasSupportUserAccessAsync()
    {
        return Task.FromResult(true);
    }
}

public class CurrentUserPermissionsValidator : IPermissionValidator
{
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly IUserPermissionsProvider _userPermissionProvider;

    public CurrentUserPermissionsValidator(
        IUserDetailsProvider userDetailsProvider,
        IUserPermissionsProvider userPermissionProvider)
    {
        _userDetailsProvider = userDetailsProvider;
        _userPermissionProvider = userPermissionProvider;
    }

    public Task<bool> HasPermissionAsync(string permissionId)
    {
        return HasAtLeastOneOfSpecificPermissionAsync(permissionId);
    }

    public async Task<bool> HasAtLeastOneOfSpecificPermissionAsync(params string[] permissionIds)
    {
        if (!_userDetailsProvider.IsAuthenticatedUser)
        {
            return false;
        }

        if (_userDetailsProvider.IsSystemUser)
        {
            return true;
        }

        var currentUserId = _userDetailsProvider.AuthenticatedUser.Id;
        var currentUserPermissions = await _userPermissionProvider.GetPermissions(currentUserId);

        return permissionIds.Any(permission => currentUserPermissions.Contains(permission));
    }

    public Task<bool> HasSupportUserAccessAsync()
    {
        if (!_userDetailsProvider.IsAuthenticatedUser)
        {
            return Task.FromResult(false);
        }

        if (_userDetailsProvider.IsSystemUser)
        {
            return Task.FromResult(true);
        }

        var authenticatedUser = this._userDetailsProvider.AuthenticatedUser;
        return Task.FromResult(authenticatedUser.IsSupportUser);
    }
}