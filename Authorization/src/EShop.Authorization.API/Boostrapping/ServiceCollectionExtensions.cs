using EShop.Authorization.Application.Abstractions;
using EShop.Authorization.Application.DependencyInjections;
using EShop.Authorization.Infrastructure;
using EShop.Authorization.Infrastructure.Authentication;
using EShop.Authorization.Infrastructure.DependencyInjection;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Cache.KeyEncryption;
using EShop.Shared.Cache.Providers;
using EShop.Shared.CQRS;
using EShop.Shared.DomainTools.DependencyInjections;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.JsonApi.Middlewares;
using EShop.Shared.Scoping.DependencyInjections.Options;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace EShop.Authorization.API.Boostrapping;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddResiliencePolicy();
        services.AddMediator(Application.AssemblyReference.Assembly);

        services.AddPostgreSqlHealthCheck(configuration)
            .AddDbContextWithScoping<AuthorizationDbContext>(configuration);

        services.AddRedisHealthCheck(configuration)
            .AddRedisInfrastructure(configuration)
            .AddUserTokensProvider();

        return services;
    }

    public static IServiceCollection AddBoostrapping(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services
            .AddAuthorizationAPI()
            .AddAuthorizationApplication()
            .AddAuthorizationPersistence()
            .AddAuthorizationEventBus(configuration, environment);

        return services;
    }

    public static IServiceCollection AddAuthorizationAPI(this IServiceCollection services)
    {
        services.AddCors();
        services.AddSingleton<ExceptionHandlingMiddleware>();

        services.AddSwaggerGenNewtonsoftSupport()
            .AddFluentValidationRulesToSwagger()
            .AddEndpointsApiExplorer()
            .AddSwaggerAPI();

        services
            .AddApiVersioning(options => options.ReportApiVersions = true)
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        services.AddJwtTokenAuthentication();

        return services;
    }

    public static IServiceCollection AddJwtTokenAuthentication(this IServiceCollection services)
    {
        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.ConfigurationSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Caching providers for RSA keys
        services.AddScoped<IRedisCachingProvider<RsaKeyPair>, RedisCachingProvider<RsaKeyPair>>();
        services.AddScoped<IRedisCachingProvider<RsaPublicKeyCacheEntry>, RedisCachingProvider<RsaPublicKeyCacheEntry>>();

        // RSA Key Management
        services.AddScoped<IKeyManagerCachingService, RsaKeyManagerRedisCachingService>();
        services.AddScoped<IRsaKeyManager, RsaKeyManager>();

        // Jwt Token Management
        services.AddScoped<IJwtTokenManager, JwtTokenManager>();
        services.AddJwtTokenExtension();

        return services;
    }

    public static IServiceCollection AddJwtTokenExtension(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddBearerToken(JwtBearerDefaults.AuthenticationScheme, options =>
        {

        });

        return services;
    }
}
