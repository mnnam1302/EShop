using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.DependencyInjections;
using EShop.Shared.JsonApi.Middlewares;

namespace EShop.Catalog.Application.Shared;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ExceptionHandlingMiddleware>();

        services
            .AddPostgreSqlHealthCheck(configuration)
            .AddDbContextPoolWithScoping<CatalogDbContext>(configuration);

        services
            .AddRedisHealthCheck(configuration)
            .AddRedisInfrastructure(configuration);

        services
            .AddUserPermissionsProvider()
            .AddUserOrganizationContextProvider()
            .AddTenantFeaturesProvider();

        services.AddCQRS(AssemblyReference.Assembly);

        return services;
    }
}
