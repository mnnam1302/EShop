using EShop.Tenancy.Domain;
using EShop.Tenancy.Domain.Aggregates;
using EShop.Tenancy.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Tenancy.Persistence.DependencyInjections;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddTenancyPersistence(this IServiceCollection services)
    {
        services.AddTransient<DbInitializer>();

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<ITenancyUnitOfWork, TenancyUnitOfWork>();

        return services;
    }
}