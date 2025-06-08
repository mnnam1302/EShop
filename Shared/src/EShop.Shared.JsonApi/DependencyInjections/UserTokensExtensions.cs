using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.Contracts.Services.Identity.Auth;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static EShop.Shared.Contracts.Services.Identity.Auth.Response;

namespace EShop.Shared.JsonApi.DependencyInjections;

public static class UserTokensExtensions
{
    public static IServiceCollection AddUserTokensProvider(this IServiceCollection services, IConfiguration configuration)
    {
        AddTokenCachingService(services, configuration);
        return services;
    }

    private static void AddTokenCachingService(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IRedisCachingAsyncProvider<AuthenticatedResponse>, RedisCachingAsyncProvider<AuthenticatedResponse>>();
        services.AddTransient<IUserTokenCachingService, TokenRedisCachingService>();
    }
}