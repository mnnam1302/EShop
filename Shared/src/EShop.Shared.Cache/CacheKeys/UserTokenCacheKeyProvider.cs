namespace EShop.Shared.Cache.CacheKeys;

public static class UserTokenCacheKeyProvider
{
    private const string OwnerService = "authorization";

    public static string GetCacheKey(string userId)
    {
        return string.Format("{0}:tokens:user:{1}", OwnerService, userId);
    }

    /// <summary>
    /// Gets the legacy (non-scoped) cache key format for migration compatibility.
    /// This will be removed in a future version after all tokens have migrated to the new format.
    /// </summary>
    public static string GetLegacyCacheKey(string userId)
    {
        return string.Format("{0}:tokens:{1}", OwnerService, userId);
    }
}