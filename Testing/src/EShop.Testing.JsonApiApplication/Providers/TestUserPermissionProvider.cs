using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;

namespace EShop.Testing.JsonApiApplication.Providers;

public sealed class TestUserPermissionProvider : IUserPermissionsProvider
{
    private readonly Dictionary<string, List<string>> userPermissions = new Dictionary<string, List<string>>();

    public void AddPermission(string userId, string permissionId)
    {
        var userIdKey = userId.ToLower();

        if (!userPermissions.ContainsKey(userIdKey))
        {
            userPermissions.Add(userIdKey, new List<string>());
        }

        if (!userPermissions[userIdKey].Contains(permissionId))
        {
            userPermissions[userIdKey].Add(permissionId);
        }
    }

    public async Task<string[]> GetPermissions(string userId, CancellationToken cancellationToken = default)
    {
        var userIdKey = userId.ToLower();
        var permissions = userPermissions.ContainsKey(userIdKey)
                ? [.. userPermissions[userIdKey]]
                : Array.Empty<string>();

        return await Task.FromResult(permissions);
    }

    public void ClearAll()
    {
        userPermissions.Clear();
    }
}