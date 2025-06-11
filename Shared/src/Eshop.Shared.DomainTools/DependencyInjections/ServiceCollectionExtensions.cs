using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.DomainTools.DependencyInjections;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddResiliencePolicy(this IServiceCollection services)
    {
        services.AddSingleton<IResiliencePolicyFactory, ResiliencePolicyFactory>();
        return services;
    }
}