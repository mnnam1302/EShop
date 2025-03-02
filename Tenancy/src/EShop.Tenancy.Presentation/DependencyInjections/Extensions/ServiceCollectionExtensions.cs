using Carter;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Tenancy.Presentation.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTenancyPresentation(this IServiceCollection services)
    {
        services.AddCarter();
        return services;
    }
}