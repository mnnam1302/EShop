using EShop.Shared.Scoping.ResourceAccessControl.Providers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly.CircuitBreaker;
using System.Text;

namespace EShop.Shared.Cache
{
    public interface IRedisCachingProvider<TValue> where TValue : class
    {
        void AddValue(string cacheKey, TValue value);

        void ClearCache(string cacheKey);

        TValue? GetValue(string cacheKey);
    }

    public class RedisCachingProvider<TValue> : IRedisCachingProvider<TValue>
        where TValue : class
    {
        private readonly CachedRemoteConfiguration cachedRemoteConfiguration;
        private readonly IDistributedCache distributedCache;
        private readonly IRedisResiliencePolicyProvider resiliencePolicyProvider;
        private readonly ILogger<RedisCachingProvider<TValue>> logger;

        public RedisCachingProvider(
            CachedRemoteConfiguration cachedRemoteConfiguration,
            IDistributedCache distributedCache,
            IRedisResiliencePolicyProvider resiliencePolicyProvider,
            ILogger<RedisCachingProvider<TValue>> logger)
        {
            this.cachedRemoteConfiguration = cachedRemoteConfiguration;
            this.distributedCache = distributedCache;
            this.resiliencePolicyProvider = resiliencePolicyProvider;
            this.logger = logger;
        }

        public void AddValue(string cacheKey, TValue value)
        {
            var slidingExpiration = cachedRemoteConfiguration.GetSlidingExpiration();

            try
            {
                var cacheValue = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));

                var contextData = new Dictionary<string, object>
                {
                    [PolicyContextItems.Logger] = logger
                };

                resiliencePolicyProvider
                    .RedisRetryPolicy
                    .Wrap(resiliencePolicyProvider.RedisCircuitBreakerPolicy)
                    .Execute(_ =>
                    {
                        // Best practise: Remove before add to avoid WRONGTYPE issue
                        distributedCache.Remove(cacheKey);
                        distributedCache.Set(
                            cacheKey,
                            cacheValue,
                            new DistributedCacheEntryOptions { SlidingExpiration = slidingExpiration });
                    },
                    contextData);
            }
            catch (StackExchange.Redis.RedisConnectionException ex)
            {
                logger.LogWarning(ex, "RedisConnectionException while setting cache value after maximum retry attempted");
            }
            catch (StackExchange.Redis.RedisTimeoutException ex)
            {
                logger.LogWarning(ex, "RedisTimeoutException while setting cache value after maximum retry attempted");
            }
            catch (BrokenCircuitException)
            {
                logger.LogWarning("Bypassing cache, circuit broken");
            }
            catch (StackExchange.Redis.RedisCommandException ex)
                when (ex.Message.Contains("Command cannot be issued to a slave", StringComparison.InvariantCultureIgnoreCase)
                    || ex.Message.Contains("Command cannot be issued to a replica", StringComparison.InvariantCultureIgnoreCase))
            {
                logger.LogWarning(ex, "RedisCommandException - assuming endpoints not refreshed yet");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception while accessing Redis cache");
            }
        }

        public void ClearCache(string cacheKey)
        {
            logger.LogDebug("Clear distributed cache '{cacheKey}'", cacheKey);

            distributedCache.Remove(cacheKey);
        }

        public TValue? GetValue(string cacheKey)
        {
            try
            {
                byte[]? encodedCache = null;

                var contextData = new Dictionary<string, object>
                {
                    [PolicyContextItems.Logger] = logger
                };

                resiliencePolicyProvider
                    .RedisRetryPolicy
                    .Wrap(resiliencePolicyProvider.RedisCircuitBreakerPolicy)
                    .Execute(_ =>
                    {
                        encodedCache = distributedCache.Get(cacheKey);
                    },
                    contextData);

                if (encodedCache != null)
                {
                    return JsonConvert.DeserializeObject<TValue>(Encoding.UTF8.GetString(encodedCache));
                }
                return default;
            }
            catch (StackExchange.Redis.RedisConnectionException ex)
            {
                logger.LogWarning(ex, "RedisConnectionException while getting cache value after maximum retry attempted");

                return default;
            }
            catch (StackExchange.Redis.RedisTimeoutException ex)
            {
                logger.LogWarning(ex, "RedisTimeoutException while getting cache value after maximum retry attempted");

                return default;
            }
            catch (BrokenCircuitException)
            {
                logger.LogWarning("Bypassing cache, circuit broken");
                return default;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception while accessing Redis cache");
                return default;
            }
        }
    }
}