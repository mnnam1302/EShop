using EShop.Authorization.API.Middlewares;
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
        services.AddMultiTenantAuthentication();

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

        return services;
    }

    public static IServiceCollection AddMultiTenantAuthentication(this IServiceCollection services)
    {
        // Configure options with validation
        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.ConfigurationSection)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Register RSA key management services
        services.AddScoped<IRedisCachingProvider<RsaKeyPair>, RedisCachingProvider<RsaKeyPair>>();
        services.AddScoped<IRedisCachingProvider<RsaPublicKeyCacheEntry>, RedisCachingProvider<RsaPublicKeyCacheEntry>>();
        services.AddScoped<IKeyManagerCachingService, RsaKeyManagerRedisCachingService>();
        services.AddScoped<IRsaKeyManager, RsaKeyManager>();

        // Register background services
        //services.AddHostedService<RsaKeyRotationBackgroundService>();

        // Register JWT token management
        services.AddScoped<IJwtTokenManager, JwtTokenManager>();
        services.AddJwtTokenAuthentication();

        return services;
    }

    public static IServiceCollection AddJwtTokenAuthentication(this IServiceCollection services)
    {
        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddScheme<JwtBearerOptions, MultiTenantJwtBearerHandler>(JwtBearerDefaults.AuthenticationScheme, options => { });

        services.AddAuthorization();

        return services;
    }
}
