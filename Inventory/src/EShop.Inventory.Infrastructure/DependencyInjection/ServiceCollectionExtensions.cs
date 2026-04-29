using Microsoft.Extensions.DependencyInjection;

namespace EShop.Inventory.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInventoryPersistence(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services)
    {
        return services;
    }
}