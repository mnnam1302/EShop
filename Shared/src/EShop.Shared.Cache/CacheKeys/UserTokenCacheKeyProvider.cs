namespace EShop.Shared.Cache.CacheKeys;

public static class UserTokenCacheKeyProvider
{
    private const string OwnerService = "users";

    public static string GetCacheKey(string userId)
    {
        return string.Format("{0}:tokens:{1}", OwnerService, userId);
    }
}