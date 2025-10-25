using Carter;
using EShop.Shared.Authentication.DependencyInjections;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.DomainTools.DependencyInjections;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.JsonApi.Middlewares;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;
using EShop.Tenancy.Application.DependencyInjections.Extensions;
using EShop.Tenancy.Application.Services;
using EShop.Tenancy.Infrastructure.DependencyInjections;
using EShop.Tenancy.Persistence;
using EShop.Tenancy.Persistence.DependencyInjections.Extensions;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;

namespace EShop.Tenancy.API.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddResiliencePolicy();

        // PostgreSQL
        services
            .AddPostgreSqlHealthCheck(configuration)
            .AddDbContextWithScoping<TenancyDbContext>(configuration);

        // Redis infrastructure
        services
            .AddRedisHealthCheck(configuration)
            .AddRedisInfrastructure(configuration);

        // Providers
        services.AddUserPermissionsProvider();
        services.AddUserOrganizationContextProvider();

        return services;
    }

    public static IServiceCollection AddBoostrapping(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services
            .AddTenancyAPI()
            .AddTenancyApplication()
            .AddTenancyPersistence()
            .AddTenancyInfrastructure(configuration, environment, Program.ApplicationName);

        return services;
    }

    private static IServiceCollection AddTenancyAPI(this IServiceCollection services)
    {
        services.AddCors();
        services.AddSingleton<ExceptionHandlingMiddleware>();
        services.AddTransient<DbInitializer>();

        services.AddCarter();

        services
            .AddSwaggerGenNewtonsoftSupport()
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

        services.AddTenantFeaturesProviderForOwnerService();
        services.AddRsaKeyServices();
        services.AddAuthentication();

        return services;
    }

    private static void AddTenantFeaturesProviderForOwnerService(this IServiceCollection services)
    {
        services.AddScoped<IFeatureValidator, CurrentUserFeaturesValidator>();
        services.AddScoped<ITenantFeaturesProvider, OwnerTenantFeaturesProvider>();
        services.AddScoped<ITenantFeaturesCachingService, TenantFeaturesRedisCachingService>();
        services.AddScoped<IRedisCachingProvider<string[]>, RedisCachingProvider<string[]>>();
        services.AddScoped<IFeatureService, FeatureService>();
    }

    public static void AddRsaKeyServices(this IServiceCollection services)
    {
        services.AddMultiTenantKeyManager();
        services.AddRsaKeyCachingProvider();
    }

    public static void AddAuthentication(this IServiceCollection services)
    {
        services.AddJwtTokenAuthentication();
        services.AddUserTokensProvider();
    }
}