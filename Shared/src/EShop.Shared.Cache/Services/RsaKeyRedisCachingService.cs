using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Cache.CacheKeys;
using EShop.Shared.Cache.Providers;
using Microsoft.Extensions.Caching.Distributed;

namespace EShop.Shared.Cache.Services
{
    internal sealed class RsaKeyRedisCachingService : IRsaKeyCachingService
    {
        private readonly IRedisCachingProvider<RsaKeyPair> _redisCachingProvider;

        public RsaKeyRedisCachingService(IRedisCachingProvider<RsaKeyPair> redisCachingProvider)
        {
            _redisCachingProvider = redisCachingProvider;
        }

        public async Task AddAsync(string tenantId, RsaKeyPair keyPair, CancellationToken cancellationToken)
        {
            var cacheKey = RsaCacheKeyProvider.GetRsaKeyPairCacheKey(tenantId);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = keyPair.ExpiresAt
            };

            await _redisCachingProvider.AddAsync(cacheKey, keyPair, options, cancellationToken);
        }

        public async Task<RsaKeyPair?> GetAsync(string tenantId, CancellationToken cancellationToken)
        {
            var cacheKey = RsaCacheKeyProvider.GetRsaKeyPairCacheKey(tenantId);

            return await _redisCachingProvider.GetAsync(cacheKey, cancellationToken);
        }

        public Task RemoveAsync(string tenantId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
