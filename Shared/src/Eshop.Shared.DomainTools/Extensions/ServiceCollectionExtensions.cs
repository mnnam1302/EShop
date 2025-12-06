using Microsoft.Extensions.DependencyInjection;

namespace EShop.Shared.DomainTools.Extensions;

public static class ServiceCollectionExtensionss
{
    public static IServiceCollection AddResiliencePolicy(this IServiceCollection services)
    {
        services.AddSingleton<IResiliencePolicyFactory, ResiliencePolicyFactory>();
        return services;
    }
}