using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Authentication.DependencyInjections;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.Cache.DependencyInejctions.Extensions;

public static class RsaKeyCacheExtensions
{
    public static IServiceCollection AddRsaKeyCachingServices(this IServiceCollection services)
    {
        services.AddOptions<TenantKeyOptions>()
            .BindConfiguration(nameof(TenantKeyOptions))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<ITenantKeyCachingService, TenantKeyRedisCachingService>();
        services.AddScoped<IRedisCachingProvider<RsaKeyPair>, RedisCachingProvider<RsaKeyPair>>();
        return services;
    }
}
