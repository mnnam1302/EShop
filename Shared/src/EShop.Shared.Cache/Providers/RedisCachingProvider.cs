using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace EShop.Shared.Cache.Providers;

public interface IRedisCachingProvider<TValue> where TValue : class
{
    Task AddAsync(string cacheKey, TValue value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default);

    Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default);

    Task<TValue?> GetAsync(string cacheKey, CancellationToken cancellationToken = default);
}

public sealed class RedisCachingProvider<TValue> : IRedisCachingProvider<TValue> where TValue : class
{
    private readonly ILogger _logger;
    private readonly IDistributedCache _distributedCache;
    private readonly IRedisResiliencePolicyProvider _resiliencePolicyProvider;
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public RedisCachingProvider(
        ILogger<RedisCachingProvider<TValue>> logger,
        IDistributedCache distributedCache,
        IRedisResiliencePolicyProvider resiliencePolicyProvider)
    {
        _logger = logger;
        _distributedCache = distributedCache;
        _resiliencePolicyProvider = resiliencePolicyProvider;
    }

    public async Task AddAsync(string cacheKey, TValue value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default)
    {
        var cacheValue = SerializeValueForCaching(value);
        var contextData = CreatePollyContextData();

        await _resiliencePolicyProvider
            .RedisRetryPolicy
            .Wrap(_resiliencePolicyProvider.RedisCircuitBreakerPolicy)
            .Execute(async (_, pollyCancellationToken) =>
            {
                await _distributedCache.SetAsync(cacheKey, cacheValue, options, pollyCancellationToken);
            },
            contextData,
            cancellationToken);
    }

    private static byte[] SerializeValueForCaching(TValue value)
    {
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, _jsonSerializerOptions));
    }

    public async Task RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        var contextData = CreatePollyContextData();

        await _resiliencePolicyProvider
            .RedisRetryPolicy
            .Wrap(_resiliencePolicyProvider.RedisCircuitBreakerPolicy)
            .Execute(async (_, pollyCancellationToken) =>
            {
                _logger.LogDebug("Removed distributed cache '{CacheKey}'", cacheKey);
                await _distributedCache.RemoveAsync(cacheKey, pollyCancellationToken);
            },
            contextData,
            cancellationToken);
    }

    public async Task<TValue?> GetAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        byte[]? valueFromCache = null;
        var contextData = CreatePollyContextData();

        await _resiliencePolicyProvider
            .RedisRetryPolicy
            .Wrap(_resiliencePolicyProvider.RedisCircuitBreakerPolicy)
            .Execute(async (_, pollyCancellationToken) =>
            {
                valueFromCache = await _distributedCache.GetAsync(cacheKey, pollyCancellationToken);
            },
            contextData,
            cancellationToken);

        return DeserializeCachedValue(valueFromCache);
    }

    private static TValue? DeserializeCachedValue(byte[]? value)
    {
        return value is null ? default : JsonSerializer.Deserialize<TValue>(Encoding.UTF8.GetString(value), _jsonSerializerOptions);
    }

    private Dictionary<string, object> CreatePollyContextData()
    {
        return new() { [PolicyContextItems.Logger] = _logger };
    }
}
