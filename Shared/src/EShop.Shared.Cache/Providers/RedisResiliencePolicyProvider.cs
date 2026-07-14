using Polly;

namespace EShop.Shared.Cache.Providers;

public interface IRedisResiliencePolicyProvider
{
    Policy RedisRetryPolicy { get; }
    Policy RedisCircuitBreakerPolicy { get; }
    IAsyncPolicy RateLimiterTimeoutPolicy { get; }
    IAsyncPolicy RateLimiterCircuitBreakerPolicy { get; }
}

public sealed class RedisResiliencePolicyProvider : IRedisResiliencePolicyProvider
{
    public Policy RedisRetryPolicy => PollyPolicies.RedisRetryPolicy;

    public Policy RedisCircuitBreakerPolicy => PollyPolicies.RedisCircuitBreakerPolicy;

    public IAsyncPolicy RateLimiterTimeoutPolicy => PollyPolicies.RateLimiterTimeoutPolicy;

    public IAsyncPolicy RateLimiterCircuitBreakerPolicy => PollyPolicies.RateLimiterCircuitBreakerPolicy;
}