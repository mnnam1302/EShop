using EShop.Shared.Diagnostics;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using StackExchange.Redis;

namespace EShop.Shared.Cache.Providers;

public static class PollyPolicies
{
    private const int _retryCount = 2;
    private const int _exceptionsAllowedBeforeBreaking = 3;
    private const int _durationOfBreakInSeconds = 60;

    // Distributed rate-limiter checks run on the request hot path: a strict timeout so
    // a slow/overloaded Redis never adds meaningful latency, and a circuit breaker that trips fast
    // (fewer allowed failures, shorter break) so the gateway stops paying the timeout cost per request
    // once Redis is genuinely down, falling open to the in-memory Layer-0 limiter instead.
    private const int _rateLimiterTimeoutMilliseconds = 50;
    private const int _rateLimiterExceptionsAllowedBeforeBreaking = 5;
    private const int _rateLimiterDurationOfBreakInSeconds = 15;

    // Pessimistic (not Optimistic): StackExchange.Redis's ScriptEvaluateAsync doesn't observe a
    // CancellationToken, so Optimistic's cooperative cancellation never fires — a hung connection
    // would run until StackExchange.Redis's own ~5s internal timeout instead of this policy's 50ms.
    // Pessimistic forcibly races the call against a timer regardless of whether it cooperates.
    public static readonly AsyncTimeoutPolicy RateLimiterTimeoutPolicy =
        Policy.TimeoutAsync(TimeSpan.FromMilliseconds(_rateLimiterTimeoutMilliseconds), TimeoutStrategy.Pessimistic);

    public static readonly AsyncCircuitBreakerPolicy RateLimiterCircuitBreakerPolicy =
        Policy
            .Handle<RedisTimeoutException>()
            .Or<RedisConnectionException>()
            .Or<TimeoutRejectedException>()
            .CircuitBreakerAsync(
                _rateLimiterExceptionsAllowedBeforeBreaking,
                TimeSpan.FromSeconds(_rateLimiterDurationOfBreakInSeconds),
                (exception, timespan, context) =>
                {
                    if (context.TryGetLogger(out var logger) && logger is not null)
                    {
                        logger.LogWarning(LogEvents.RedisCircuitBreakerActivated, exception, "Rate limiter Redis circuit breaker activated due to server connection issues");
                    }
                },
                context =>
                {
                    if (context.TryGetLogger(out var logger) && logger is not null)
                    {
                        logger.LogInformation("Rate limiter Redis circuit breaker deactivated, connection back and working");
                    }
                });

    public static readonly RetryPolicy RedisRetryPolicy =
        Policy
            .Handle<RedisTimeoutException>()
            .Or<RedisConnectionException>()
            .Retry(
                _retryCount,
                (exception, retryAttempt, context) =>
                {
                    if (context.TryGetLogger(out var logger) && logger is not null)
                    {
                        logger.LogDebug(exception,
                            "RedisTimeoutException/RedisConnectionException handled, Retry number '{Current}/{Max}'",
                            retryAttempt, _retryCount);
                    }
                });

    public static readonly CircuitBreakerPolicy RedisCircuitBreakerPolicy =
        Policy
            .Handle<RedisTimeoutException>()
            .Or<RedisConnectionException>()
            .CircuitBreaker(_exceptionsAllowedBeforeBreaking, TimeSpan.FromSeconds(_durationOfBreakInSeconds),
            (exception, timespan, context) =>
            {
                if (context.TryGetLogger(out var logger) && logger is not null)
                {
                    logger.LogWarning(LogEvents.RedisCircuitBreakerActivated, exception, "Redis circuit breaker activated due to server connection issues");
                }
            },
            context =>
            {
                if (context.TryGetLogger(out var logger) && logger is not null)
                {
                    logger.LogInformation("Redis circuit breaker deactivated, connection back and working");
                }
            });
}
