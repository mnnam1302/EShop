namespace EShop.Shared.Cache;

public static class UserPermissionCacheKeyProvider
{
    private const string OwnerService = "users";

    public static string GetCacheKey(string userId)
    {
        return string.Format("{0}:permissions:{1}", OwnerService, userId);
    }
}