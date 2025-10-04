using Polly;

namespace EShop.Shared.Cache.Providers;

public interface IRedisResiliencePolicyProvider
{
    Policy RedisRetryPolicy { get; }
    Policy RedisCircuitBreakerPolicy { get; }
}

public sealed class RedisResiliencePolicyProvider : IRedisResiliencePolicyProvider
{
    public Policy RedisRetryPolicy => PollyPolicies.RedisRetryPolicy;

    public Policy RedisCircuitBreakerPolicy => PollyPolicies.RedisCircuitBreakerPolicy;
}