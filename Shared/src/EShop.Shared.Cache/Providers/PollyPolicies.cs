using EShop.Shared.Diagnostics;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using StackExchange.Redis;

namespace EShop.Shared.Cache.Providers;

public static class PollyPolicies
{
    private const int _retryCount = 2;
    private const int _exceptionsAllowedBeforeBreaking = 3;
    private const int _durationOfBreakInSeconds = 60;

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
