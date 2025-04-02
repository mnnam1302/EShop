using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.DomainTools.DependencyInjections;
using EShop.Shared.JsonApi.DependencyInjections;
using EShop.Shared.JsonApi.Middlewares;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;
using EShop.Tenancy.Application.DependencyInjections.Extensions;
using EShop.Tenancy.Infrastructure.DependencyInjections.Extensions;
using EShop.Tenancy.Persistence;
using EShop.Tenancy.Persistence.DependencyInjections.Extensions;
using EShop.Tenancy.Presentation.DependencyInjections.Extensions;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;

namespace EShop.Tenancy.API.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddResiliencePolicy();

        services
            .AddPostgreSqlHealthCheck(configuration)
            .AddDbContextWithScoping<TenancyDbContext>(configuration);

        services
            .AddRedisHealthCheck(configuration)
            .AddRedisInfrastructure(configuration);

        return services;
    }

    public static IServiceCollection AddBoostrapping(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddTenancyPresentation(); // Must before API project, because contain DI Carter
        services.AddTenancyAPI();
        services.AddTenancyApplication();
        services.AddTenancyPersistence();
        services.AddTenancyInfrastructure(configuration, environment, Program.ApplicationName);

        services.AddUserPermissionsProvider(configuration);
        services.AddTenantFeaturesProviderForOwnerService(configuration);

        return services;
    }

    private static void AddTenancyAPI(this IServiceCollection services)
    {
        services.AddCors();
        services.AddSingleton<ExceptionHandlingMiddleware>();

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
        AddTenantFeatureCachingService(services);
    }

    private static void AddTenantFeatureCachingService(IServiceCollection services)
    {
        services.AddTransient<IRedisCachingAsyncProvider<string[]>, RedisCachingAsyncProvider<string[]>>();
        services.AddTransient<ITenantFeaturesCachingService, TenantFeaturesRedisCachingService>();
    }
}