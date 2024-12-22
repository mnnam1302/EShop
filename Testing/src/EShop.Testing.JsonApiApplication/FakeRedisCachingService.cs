using EShop.Shared.Cache.Services;
using Microsoft.Extensions.Caching.Distributed;

namespace EShop.Testing.JsonApiApplication;

public class FakeRedisCachingService : IRedisCachingService, IAsyncRedisCachingService
{
    private readonly Dictionary<string, object> cache = new();

    public void Add<T>(string cacheKey, T value, DistributedCacheEntryOptions options)
    {
        cache[cacheKey] = value;
    }

    public Task AddAsync<T>(string cacheKey, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default)
    {
        cache[cacheKey] = value;
        return Task.CompletedTask;
    }

    public void Clear(string cacheKey)
    {
        cache.Remove(cacheKey);
    }

    public Task ClearAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        cache.Remove(cacheKey);
        return Task.CompletedTask;
    }

    public T? Get<T>(string cacheKey)
    {
        return cache.TryGetValue(cacheKey, out var value)
            ? (T)value : default;
    }

    public Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
    {
        var returnValue = cache.TryGetValue(cacheKey, out var value)
            ? (T)value
            : default;

        return Task.FromResult(returnValue);
    }
}