using EShop.Shared.Diagnostics;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using StackExchange.Redis;

namespace EShop.Shared.Cache.Providers;

public static class PollyPolicies
{
    private const int RetryCount = 2;
    private const int ExceptionsAllowedBeforeBreaking = 3;
    private const int DurationOfBreakInSeconds = 60;

    public static readonly RetryPolicy RedisRetryPolicy =
        Policy
            .Handle<RedisTimeoutException>()
            .Or<RedisConnectionException>()
            .Retry(
                RetryCount,
                (exception, retryAttempt, context) =>
                {
                    if (context.TryGetLogger(out var logger))
                    {
                        logger.LogDebug(exception, "RedisTimeoutException/RedisConnectionException handled, Retry number {current}/{max}'", retryAttempt, RetryCount);
                    }
                });

    public static readonly CircuitBreakerPolicy RedisCircuitBreakerPolicy =
        Policy
            .Handle<RedisTimeoutException>()
            .Or<RedisConnectionException>()
            .CircuitBreaker(ExceptionsAllowedBeforeBreaking, TimeSpan.FromSeconds(DurationOfBreakInSeconds),
            (exception, timespan, context) =>
            {
                if (context.TryGetLogger(out var logger))
                {
                    logger.LogWarning(LogEvents.RedisCircuitBreakerActivated, exception, "Redis circuit breaker activated due to server connection issues");
                }
            },
            context =>
            {
                if (context.TryGetLogger(out var logger))
                {
                    logger.LogInformation("Redis circuit breaker deactivated, connection back and working");
                }
            });
}