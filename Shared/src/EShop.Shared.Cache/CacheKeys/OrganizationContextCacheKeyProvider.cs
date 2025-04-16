namespace EShop.Shared.Cache.CacheKeys;

public static class OrganizationContextCacheKeyProvider
{
    private const string OwnerService = "users";

    public static string GetUserOrganizationContextCacheKey(string userId, string userType)
    {
        return string.Format("{0}:userOrganizationContext:{1}:{2}", OwnerService, userId, userType);
    }

    public static string GetOrganizationContextCacheKey(string organizationId)
    {
        return string.Format("{0}:organizationContext:{1}", OwnerService, organizationId);
    }
}