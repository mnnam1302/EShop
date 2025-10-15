using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.JsonApi.Extensions;

public static class UserTokensExtensions
{
    public static IServiceCollection AddUserTokensProvider(this IServiceCollection services)
    {
        services.AddTransient<IRedisCachingProvider<TokenAuthentication>, RedisCachingProvider<TokenAuthentication>>();
        services.AddTransient<IUserTokenCachingService, UserTokenRedisCachingService>();

        return services;
    }
}