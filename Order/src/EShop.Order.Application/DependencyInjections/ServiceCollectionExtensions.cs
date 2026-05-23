using EShop.Shared.CQRS;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Order.Application.DependencyInjections;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderApplication(this IServiceCollection services)
    {
        services.AddMediator(AssemblyReference.Assembly);
        return services;
    }
}
