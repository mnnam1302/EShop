using EShop.Inventory.Domain.Abstractions;
using EShop.Inventory.Infrastructure.Repositories;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.JsonApi.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Inventory.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInventoryPersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString(environment);
        services
            .AddPostgreSqlHealthCheck(configuration, connectionString)
            .AddDbContextWithScoping<InventoryDbContext>(configuration, connectionString, useRingFencedScoping: false)
            .AddRepositoryUnitOfWork();

        return services;
    }

    private static IServiceCollection AddRepositoryUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, EFUnitOfWork<InventoryDbContext>>();
        services
            .AddScoped<IInventoryRepository, InventoryRepository>();
        return services;
    }

    public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRedis(configuration);
        return services;
    }

    private static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddRedisHealthCheck(configuration)
            .AddRedisCacheInfrastructure(configuration);

        return services;
    }
}
