using EShop.Shared.Cache.Providers;
using EShop.Shared.Diagnostics;
using EShop.Shared.RateLimiting.Abstractions;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using StackExchange.Redis;

namespace EShop.Shared.RateLimiting.Redis;

// Design: a broken/slow Redis must never become a platform outage or an unbounded flood. Every
// check goes through a ~50ms timeout + circuit breaker (PollyPolicies.RateLimiterTimeoutPolicy /
// RateLimiterCircuitBreakerPolicy); once either trips, the request is served from an in-memory
// per-node fallback rather than the exception propagating or the request being dropped.
public sealed class FailOpenRateLimiter : IRateLimiter
{
    private readonly IRateLimiter _inner;
    private readonly IAsyncPolicy _resiliencePolicy;
    private readonly InMemoryFallbackRateLimiter _fallback = new();
    private readonly ILogger<FailOpenRateLimiter> _logger;

    public FailOpenRateLimiter(IRateLimiter inner, ILogger<FailOpenRateLimiter> logger)
    {
        _inner = inner;
        _logger = logger;
        _resiliencePolicy = PollyPolicies.RateLimiterTimeoutPolicy.WrapAsync(PollyPolicies.RateLimiterCircuitBreakerPolicy);
    }

    public async Task<CombinedRateLimitResult> CheckTokenBucketsAsync(
        TokenBucketCheck primary,
        TokenBucketCheck? secondary,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _resiliencePolicy.ExecuteAsync(ct => _inner.CheckTokenBucketsAsync(primary, secondary, ct), cancellationToken);
        }
        catch (Exception ex) when (IsRedisFailure(ex))
        {
            LogFailOpen(ex);
            return await _fallback.CheckTokenBucketsAsync(primary, secondary, cancellationToken);
        }
    }

    public async Task<RateLimitResult> CheckSlidingWindowAsync(SlidingWindowCheck check, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _resiliencePolicy.ExecuteAsync(ct => _inner.CheckSlidingWindowAsync(check, ct), cancellationToken);
        }
        catch (Exception ex) when (IsRedisFailure(ex))
        {
            LogFailOpen(ex);
            return await _fallback.CheckSlidingWindowAsync(check, cancellationToken);
        }
    }

    private static bool IsRedisFailure(Exception ex) =>
        ex is RedisTimeoutException or RedisConnectionException or TimeoutRejectedException or BrokenCircuitException;

    private void LogFailOpen(Exception ex)
    {
        _logger.LogWarning(LogEvents.RateLimiterFailOpen, ex, "Rate limiter failing open to in-memory fallback due to Redis unavailability");
        RateLimiterMetrics.FailOpen.Add(1);
    }
}
