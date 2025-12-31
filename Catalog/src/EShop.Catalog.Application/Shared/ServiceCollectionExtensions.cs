using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.CQRS;
using EShop.Shared.Diagnostics;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.JsonApi.Middlewares;

namespace EShop.Catalog.Application.Shared;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddSingleton<ExceptionHandlingMiddleware>();
        services.AddMediator(AssemblyReference.Assembly);

        var connectionString = configuration.GetConnectionString(environment);
        services
            .AddPostgreSqlHealthCheck(configuration, connectionString)
            .AddDbContextWithScoping<CatalogDbContext>(configuration, connectionString, useRingFencedScoping: false)
            .AddDbContextEventSourcing<CatalogDbContext>(configuration);

        services
            .AddRedisHealthCheck(configuration)
            .AddRedisCacheInfrastructure(configuration);

        services
            .AddTenantAuthenticationProvider()
            .AddUserPermissionsProvider()
            .AddUserOrganizationContextProvider()
            .AddTenantFeaturesProvider();

        return services;
    }
}

internal static class ConnectionStringExtensions
{
    internal static string GetConnectionString(this IConfiguration configuration, IHostEnvironment environment)
    {
        return configuration.GetRlsConnectionString(configuration.GetConnectionStringName(), environment);
    }

    internal static string GetConnectionStringName(this IConfiguration configuration)
    {
        return configuration.IsRunningInAspire() ? "catalogDatabase" : "DefaultConnection";
    }
}