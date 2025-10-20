using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Authentication.DependencyInjections;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.Cache.DependencyInejctions.Extensions
{
    public static class RsaKeyCacheExtensions
    {
        public static IServiceCollection AddRsaKeyCachingProvider(this IServiceCollection services)
        {
            services.AddOptions<RsaKeyOptions>()
                .BindConfiguration(nameof(RsaKeyOptions))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddScoped<IRsaKeyCachingService, RsaKeyRedisCachingService>();
            services.AddScoped<IRedisCachingProvider<RsaKeyPair>, RedisCachingProvider<RsaKeyPair>>();
            return services;
        }
    }
}
