using EShop.Identity.Application.Abstractions;
using EShop.Identity.Application.Services;
using EShop.Identity.Infrastructure.Authentication;
using EShop.Identity.Infrastructure.DependencyInjections.Options;
using EShop.Identity.Infrastructure.HashServices;
using EShop.Shared.Cache;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.Contracts.Services.Identity.Auth;
using EShop.Shared.Scoping.ResourceAccessControl.Providers;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Identity.Infrastructure.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddServicesInfrastructure(this IServiceCollection services)
    {
        services.AddTransient<IPasswordHasher, PasswordHasher>();
        services.AddTransient<ITokenService, TokenService>();
    }

    public static void AddRedisCachingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var redisOptions = new RedisOptions();
        configuration.GetSection(nameof(RedisOptions)).Bind(redisOptions);

        services.AddSingleton(redisOptions);

        if (!redisOptions.Enabled)
            return;

        services.AddStackExchangeRedisCache(options =>
        {
            var connectionString = redisOptions.ConnectionString;
            options.Configuration = connectionString;
        });
    }
}