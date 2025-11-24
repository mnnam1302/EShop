using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.JsonApi.Middlewares;

namespace EShop.Catalog.Application.Shared;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ExceptionHandlingMiddleware>();

        // Register CQRS services first before DbContext to ensure IDomainEventsDispatcher is available
        services.AddMediator(AssemblyReference.Assembly);

        services
            .AddPostgreSqlHealthCheck(configuration)
            .AddDbContextPoolWithScoping<CatalogDbContext>(configuration);

        services.AddPostgreSQLEventSourcing<CatalogDbContext>(configuration);

        services
            .AddRedisHealthCheck(configuration)
            .AddRedisCacheInfrastructure(configuration);

        services
            .AddUserPermissionsProvider()
            .AddUserOrganizationContextProvider()
            .AddTenantFeaturesProvider();

        return services;
    }
}