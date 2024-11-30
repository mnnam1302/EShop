using EShop.Shared.Cache.Providers;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using System.Text;
using System.Text.Json;

namespace EShop.Shared.Cache.Services
{
    public interface IRedisCachingService
    {
        void Add<T>(string cacheKey, T value, DistributedCacheEntryOptions options);

        void Clear(string cacheKey);

        T? Get<T>(string cacheKey);
    }

    public interface IAsyncRedisCachingService
    {
        Task AddAsync<T>(string cacheKey, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default);

        Task ClearAsync(string cacheKey, CancellationToken cancellationToken = default);

        Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default);
    }

    public class RedisCachingService : IRedisCachingService, IAsyncRedisCachingService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IRedisResiliencePolicyProvider _resiliencePolicyProvider;
        private readonly ILogger<RedisCachingService> _logger;
        private static readonly JsonSerializerOptions jsonSerializerOptions = new(JsonSerializerDefaults.Web);

        public RedisCachingService(IDistributedCache distributedCache,
            IRedisResiliencePolicyProvider resiliencePolicyProvider,
            ILogger<RedisCachingService> logger)
        {
            _distributedCache = distributedCache;
            _resiliencePolicyProvider = resiliencePolicyProvider;
            _logger = logger;
        }

        public void Add<T>(string cacheKey, T value, DistributedCacheEntryOptions options)
        {
            ExecuteRedisOperation(() =>
            {
                var cacheValue = SerializeValueForCaching(value);
                var contextData = CreatePollyContextData();

                _resiliencePolicyProvider
                    .RedisRetryPolicy
                    .Wrap(_resiliencePolicyProvider.RedisCircuitBreakerPolicy)
                    .Execute(_ =>
                    {
                        // Best practice: Remove before add to avoid WRONGTYPE issue
                        _distributedCache.Remove(cacheKey);
                        _distributedCache.Set(cacheKey, cacheValue, options);
                    },
                    contextData);
            },
            operationName: "setting cache value");
        }

        public void Clear(string cacheKey)
        {
            ExecuteRedisOperation(() =>
            {
                _distributedCache.Remove(cacheKey);
                _logger.LogDebug("Cleared distributed cache '{cacheKey}'", cacheKey);
            },
            operationName: "clearing cache value");
        }

        public T? Get<T>(string cacheKey)
        {
            return ExecuteRedisOperation(() =>
            {
                byte[]? valueFromCache = null;
                var contextData = CreatePollyContextData();

                _resiliencePolicyProvider
                    .RedisRetryPolicy
                    .Wrap(_resiliencePolicyProvider.RedisCircuitBreakerPolicy)
                    .Execute(_ =>
                    {
                        valueFromCache = _distributedCache.Get(cacheKey);
                    },
                    contextData);

                return DeserializeCachedValue<T>(valueFromCache);
            },
            operationName: "getting cache value");
        }

        public async Task AddAsync<T>(string cacheKey, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default)
        {
            await ExecuteRedisOperationAsync(async redisCancellationToken =>
            {
                var cacheValue = SerializeValueForCaching(value);
                var contextData = CreatePollyContextData();

                await _resiliencePolicyProvider
                    .RedisRetryPolicy
                    .Wrap(_resiliencePolicyProvider.RedisCircuitBreakerPolicy)
                    .Execute(async (_, pollyCancellationToken) =>
                    {
                        // Best practice: Remove before add to avoid WRONGTYPE issue
                        await _distributedCache.RemoveAsync(cacheKey, pollyCancellationToken);
                        await _distributedCache.SetAsync(cacheKey, cacheValue, options, pollyCancellationToken);
                    },
                    contextData,
                    redisCancellationToken);
            },
            operationName: "setting cache value",
            cancellationToken);
        }

        public async Task ClearAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            await ExecuteRedisOperationAsync(async redisCancellationToken =>
            {
                await _distributedCache.RemoveAsync(cacheKey, redisCancellationToken);
                _logger.LogDebug("Cleared distributed cache '{cacheKey}'", cacheKey);
            },
            operationName: "clearing cache value",
            cancellationToken);
        }

        public async Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
        {
            return await ExecuteRedisOperationAsync(async redisCancellationToken =>
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
                    redisCancellationToken);

                return DeserializeCachedValue<T>(valueFromCache);
            },
            operationName: "getting cache value",
            cancellationToken);
        }

        private void ExecuteRedisOperation(Action operation, string operationName)
        {
            ExecuteRedisOperation(() =>
            {
                operation();
                return ValueTuple.Create();
            },
            operationName);
        }

        private T? ExecuteRedisOperation<T>(Func<T?> operation, string operationName)
        {
            try
            {
                return operation();
            }
            catch (StackExchange.Redis.RedisConnectionException ex)
            {
                _logger.LogWarning(ex, "RedisConnectionException while performing '{operationName}' after maximum retry attempted", operationName);
                return default;
            }
            catch (StackExchange.Redis.RedisTimeoutException ex)
            {
                _logger.LogWarning(ex, "RedisTimeoutException while performing '{operationName}' after maximum retry attempted", operationName);
                return default;
            }
            catch (BrokenCircuitException)
            {
                _logger.LogWarning("Bypassing cache, circuit broken");
                return default;
            }
            catch (StackExchange.Redis.RedisCommandException ex)
                when (ex.Message.Contains("Command cannot be issued to a slave", StringComparison.InvariantCultureIgnoreCase)
                      || ex.Message.Contains("Command cannot be issued to a replica", StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogWarning(ex, "RedisCommandException - assuming endpoints not refreshed yet");
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while accessing Redis cache");
                return default;
            }
        }

        private async Task ExecuteRedisOperationAsync(Func<CancellationToken, Task> operation, string operationName, CancellationToken cancellationToken)
        {
            await ExecuteRedisOperationAsync(async ctk =>
            {
                await operation(ctk);
                return ValueTuple.Create();
            },
            operationName,
            cancellationToken);
        }

        private async Task<T?> ExecuteRedisOperationAsync<T>(Func<CancellationToken, Task<T?>> operation, string operationName, CancellationToken cancellationToken)
        {
            try
            {
                return await operation(cancellationToken);
            }
            catch (StackExchange.Redis.RedisConnectionException ex)
            {
                _logger.LogWarning(ex, "RedisConnectionException while performing '{operationName}' after maximum retry attempted", operationName);
                return default;
            }
            catch (StackExchange.Redis.RedisTimeoutException ex)
            {
                _logger.LogWarning(ex, "RedisTimeoutException while performing '{operationName}' after maximum retry attempted", operationName);
                return default;
            }
            catch (BrokenCircuitException)
            {
                _logger.LogWarning("Bypassing cache, circuit broken");
                return default;
            }
            catch (StackExchange.Redis.RedisCommandException ex)
                when (ex.Message.Contains("Command cannot be issued to a slave", StringComparison.InvariantCultureIgnoreCase)
                      || ex.Message.Contains("Command cannot be issued to a replica", StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogWarning(ex, "RedisCommandException - assuming endpoints not refreshed yet");
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while accessing Redis cache");
                return default;
            }
        }

        private static byte[] SerializeValueForCaching<T>(T value)
            => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, jsonSerializerOptions));

        private static T? DeserializeCachedValue<T>(byte[]? value)
            => value is null ? default : JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(value), jsonSerializerOptions);

        private Dictionary<string, object> CreatePollyContextData() => new() { [PolicyContextItems.Logger] = _logger };
    }
}