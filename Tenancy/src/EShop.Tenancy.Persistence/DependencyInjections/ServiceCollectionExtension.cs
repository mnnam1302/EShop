using EShop.Shared.Diagnostics;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.JsonApi.Extensions;
using EShop.Tenancy.Domain.Repositories;
using EShop.Tenancy.Persistence.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EShop.Tenancy.Persistence.DependencyInjections;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddTenancyPersistence(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString(environment);

        services
            .AddPostgreSqlHealthCheck(configuration, connectionString)
            .AddDbContextWithScoping<TenancyDbContext>(configuration, connectionString, useRingFencedScoping: false)
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

internal static class ConnectionStringExtensions
{
    internal static string GetConnectionString(this IConfiguration configuration, IHostEnvironment environment)
    {
        return configuration.GetRlsConnectionString(configuration.GetConnectionStringName(), environment);
    }

    internal static string GetConnectionStringName(this IConfiguration configuration)
    {
        return configuration.IsRunningInAspire() ? "tenancyDatabase" : "DefaultConnection";
    }
}