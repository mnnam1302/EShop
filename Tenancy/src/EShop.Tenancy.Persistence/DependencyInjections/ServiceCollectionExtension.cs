using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.JsonApi.Extensions;
using EShop.Tenancy.Domain.Repositories;
using EShop.Tenancy.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Tenancy.Persistence.DependencyInjections;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddTenancyPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddPostgreSqlHealthCheck(configuration)
            .AddDbContextWithScoping<TenancyDbContext>(configuration)
            .AddPersistenceServices();

        return services;
    }

    public static void AddPersistenceServices(this IServiceCollection services)
    {
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IFeatureRepository, FeatureRepository>();
        services.AddScoped<IUnitOfWork, EFUnitOfWork<TenancyDbContext>>();
    }
}