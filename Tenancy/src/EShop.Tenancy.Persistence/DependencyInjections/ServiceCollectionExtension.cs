using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Tenancy.Domain.Repositories;
using EShop.Tenancy.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Tenancy.Persistence.DependencyInjections;

public static class ServiceCollectionExtension
{
    public static void AddTenancyPersistence(this IServiceCollection services)
    {
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IFeatureRepository, FeatureRepository>();
        services.AddScoped<IUnitOfWork, EFUnitOfWork<TenancyDbContext>>();
    }
}