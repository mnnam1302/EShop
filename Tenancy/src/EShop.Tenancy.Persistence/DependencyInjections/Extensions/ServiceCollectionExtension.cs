using EShop.Tenancy.Domain;
using EShop.Tenancy.Domain.Repositories;
using EShop.Tenancy.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Tenancy.Persistence.DependencyInjections.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddTenancyPersistence(this IServiceCollection services)
    {
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IFeatureRepository, FeatureRepository>();

        services.AddScoped<ITenancyUnitOfWork, TenancyUnitOfWork>();

        return services;
    }
}