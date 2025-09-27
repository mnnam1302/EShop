using Microsoft.Extensions.DependencyInjection;

namespace EShop.Authorization.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {

        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {

        return services;
    }

    public static IServiceCollection AddEventBus(this IServiceCollection services)
    {
        return services;
    }
}
