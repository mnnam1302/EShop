using EShop.Shared.Cache.KeyEncryption;
using EShop.Shared.Cache.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.JsonApi.Extensions;

public static class RsaKeyExtensions
{
    public static IServiceCollection AddRsaKeyProvider(this IServiceCollection services)
    {
        services.AddScoped<IRedisCachingProvider<RsaKeyPair>, RedisCachingProvider<RsaKeyPair>>();
        services.AddScoped<IRedisCachingProvider<RsaPublicKeyCacheEntry>, RedisCachingProvider<RsaPublicKeyCacheEntry>>();
        services.AddScoped<IKeyManagerCachingService, RsaKeyManagerRedisCachingService>();

        return services;
    }
}
