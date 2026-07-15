using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Behaviors;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;
using EShop.Tenancy.Application.DependencyInjections;
using EShop.Tenancy.Application.UseCases.Features;
using EShop.Tenancy.Application.UseCases.Tenants.GetTenantFeatures;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Tenancy.Application.DependencyInjections;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTenancyApplication(this IServiceCollection services)
    {
        services.AddMediator(AssemblyReference.Assembly);

        services.AddOwnerTenantFeaturesProvider();

        return services;
    }

    public static void AddOwnerTenantFeaturesProvider(this IServiceCollection services)
    {
        services.AddScoped<IFeatureValidator, CurrentUserFeaturesValidator>();
        services.AddScoped<ITenantFeaturesProvider, OwnerTenantFeaturesProvider>();
        services.AddScoped<ITenantFeaturesCachingService, TenantFeaturesRedisCachingService>();
        services.AddScoped<IRedisCachingProvider<string[]>, RedisCachingProvider<string[]>>();
        services.AddScoped<IFeatureService, FeatureService>();
    }
}
