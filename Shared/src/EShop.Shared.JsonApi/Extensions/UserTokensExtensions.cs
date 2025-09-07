using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;
using Microsoft.Extensions.DependencyInjection;
using static EShop.Shared.Contracts.Services.Identity.Auth.Response;

namespace EShop.Shared.JsonApi.Extensions;

public static class UserTokensExtensions
{
    public static IServiceCollection AddUserTokensProvider(this IServiceCollection services)
    {
        AddTokenCachingService(services);
        return services;
    }

    private static void AddTokenCachingService(IServiceCollection services)
    {
        services.AddTransient<IRedisCachingAsyncProvider<AuthenticatedResponse>, RedisCachingAsyncProvider<AuthenticatedResponse>>();
        services.AddTransient<IUserTokenCachingService, TokenRedisCachingService>();
    }
}