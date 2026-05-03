using EShop.Shared.EventBus.DependencyInjections.Extensions;
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
            .AddPostgreSqlIdempotentConsumer<InventoryDbContext>();

        return services;
    }

    public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services)
    {
        return services;
    }
}