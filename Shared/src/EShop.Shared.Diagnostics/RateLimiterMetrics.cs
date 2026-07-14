using System.Diagnostics.Metrics;

namespace EShop.Shared.Diagnostics;

public static class RateLimiterMetrics
{
    public const string MeterName = "EShop.RateLimiter";

    private static readonly Meter Meter = new(MeterName);

    public static readonly Counter<long> Requests = Meter.CreateCounter<long>(
        "rate_limiter.requests",
        description: "Rate-limit decisions, tagged by decision (allow/reject/shadow_reject), layer, tenant, and domain.");

    public static readonly Histogram<double> RedisLatency = Meter.CreateHistogram<double>(
        "rate_limiter.redis_latency",
        unit: "ms",
        description: "Latency of the Redis EVALSHA call backing a rate-limit check.");

    public static readonly Counter<long> FailOpen = Meter.CreateCounter<long>(
        "rate_limiter.fail_open",
        description: "Occurrences of the rate limiter failing open to the in-memory, per-node fallback.");
}
