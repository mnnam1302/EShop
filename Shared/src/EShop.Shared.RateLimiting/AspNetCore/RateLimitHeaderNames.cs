namespace EShop.Shared.RateLimiting.AspNetCore;

public static class RateLimitHeaderNames
{
    public const string Limit = "RateLimit-Limit";
    public const string Remaining = "RateLimit-Remaining";
    public const string Reset = "RateLimit-Reset";
}
