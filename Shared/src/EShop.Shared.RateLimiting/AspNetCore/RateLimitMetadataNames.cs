using System.Threading.RateLimiting;

namespace EShop.Shared.RateLimiting.AspNetCore;

public static class RateLimitMetadataNames
{
    public static readonly MetadataName<long> Remaining = new("RATELIMIT_REMAINING");
    public static readonly MetadataName<string> ExceededScope = new("RATELIMIT_EXCEEDED_SCOPE");
}
