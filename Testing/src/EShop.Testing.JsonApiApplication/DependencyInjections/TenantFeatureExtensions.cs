using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;
using EShop.Testing.JsonApiApplication.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Testing.JsonApiApplication.DependencyInjections;

public static class TenantFeatureExtensions
{
    public static IServiceCollection AddTestTenantFeatures(this IServiceCollection services)
    {
        services.AddScoped<IFeatureValidator, CurrentUserFeaturesValidator>();
        services.AddSingleton<ITenantFeaturesProvider, TestTenantFeatureProvider>();

        return services;
    }
}