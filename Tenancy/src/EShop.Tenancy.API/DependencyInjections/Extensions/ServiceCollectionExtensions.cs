using Carter;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.DomainTools.DependencyInjections;
using EShop.Shared.JsonApi.DependencyInjections;
using EShop.Shared.JsonApi.Middlewares;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;
using EShop.Tenancy.Application.DependencyInjections.Extensions;
using EShop.Tenancy.Application.Services;
using EShop.Tenancy.Infrastructure.DependencyInjections.Extensions;
using EShop.Tenancy.Persistence;
using EShop.Tenancy.Persistence.DependencyInjections.Extensions;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;

namespace EShop.Tenancy.API.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddUserScoping();
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
        services
            .AddUserPermissionsProvider()
            .AddUserOrganizationContextProvider();

        return services;
    }

    public static IServiceCollection AddBoostrapping(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Clean architecture
        services.AddTenancyAPI();
        services.AddTenancyApplication();
        services.AddTenancyPersistence();
        services.AddTenancyInfrastructure(configuration, environment, Program.ApplicationName);

        // Owner services
        services.AddTenantFeaturesProviderForOwnerService(configuration);

        return services;
    }

    private static void AddTenancyAPI(this IServiceCollection services)
    {
        services.AddCors();
        services.AddSingleton<ExceptionHandlingMiddleware>();

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
    }

    private static void AddTenantFeaturesProviderForOwnerService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IFeatureValidator, CurrentUserFeaturesValidator>();
        services.AddScoped<ITenantFeaturesProvider, OwnerTenantFeaturesProvider>();
        services.AddScoped<ITenantFeaturesCachingService, TenantFeaturesRedisCachingService>();
        services.AddScoped<IRedisCachingAsyncProvider<string[]>, RedisCachingAsyncProvider<string[]>>();
        services.AddScoped<IFeatureService, FeatureService>();
    }
}