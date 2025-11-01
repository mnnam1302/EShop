using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.Cache.DependencyInejctions.Extensions;

public static class UserTokenCacheExtensions
{
    public static IServiceCollection AddUserTokensCachingServices(this IServiceCollection services)
    {
        services.AddScoped<IRedisCachingProvider<TokenAuthentication>, RedisCachingProvider<TokenAuthentication>>();
        services.AddScoped<IUserTokenCachingService, UserTokenRedisCachingService>();

        return services;
    }
}
