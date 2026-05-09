using Microsoft.Extensions.DependencyInjection;

namespace EShop.Order.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderPersistence(this IServiceCollection services)
    {
        return services;
    }

    public static IServiceCollection AddOrderInfrastructure(this IServiceCollection services)
    {
        return services;
    }
}