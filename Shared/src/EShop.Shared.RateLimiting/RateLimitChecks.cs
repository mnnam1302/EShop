namespace EShop.Shared.RateLimiting;

public sealed record TokenBucketCheck(string Key, int Capacity, int RefillTokensPerPeriod, TimeSpan RefillPeriod);

public sealed record SlidingWindowCheck(string Key, int Limit, TimeSpan Window);
