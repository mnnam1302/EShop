using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.CQRS;
using EShop.Shared.DomainTools.DependencyInjections;
using EShop.Shared.JsonApi.Extensions;

namespace EShop.Authorization.Application.Shared;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddResiliencePolicy();

        // database
        services
            .AddPostgreSqlHealthCheck(configuration)
            .AddDbContextPoolWithScoping<AuthorizationDbContext>(configuration);

        // caching
        services
            .AddRedisHealthCheck(configuration)
            .AddRedisInfrastructure(configuration);

        // providers
        services
            .AddUserTokensProvider()
            .AddTenantFeaturesProvider();

        services.AddCQRS(AssemblyReference.Assembly);

        return services;
    }
}
