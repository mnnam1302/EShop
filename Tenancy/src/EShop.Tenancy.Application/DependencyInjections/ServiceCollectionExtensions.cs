using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Behaviors;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;
using EShop.Tenancy.Application.DependencyInjections;
using EShop.Tenancy.Application.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Tenancy.Application.DependencyInjections;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTenancyApplication(this IServiceCollection services)
    {
        services.AddMediatR();
        services.AddMediator(AssemblyReference.Assembly);

        services.AddUserPermissionsProvider();
        services.AddUserOrganizationContextProvider();
        services.AddTenantFeaturesProvider();

        return services;
    }

    public static void AddMediatR(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblies(AssemblyReference.Assembly))
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>))
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformancePipelineBehavior<,>))
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(TracingPipelineBehavior<,>))
                .AddValidatorsFromAssembly(Shared.Contracts.AssemblyReference.Assembly, includeInternalTypes: true);
    }

    public static void AddTenantFeaturesProvider(this IServiceCollection services)
    {
        services.AddScoped<IFeatureValidator, CurrentUserFeaturesValidator>();
        services.AddScoped<ITenantFeaturesProvider, OwnerTenantFeaturesProvider>();
        services.AddScoped<ITenantFeaturesCachingService, TenantFeaturesRedisCachingService>();
        services.AddScoped<IRedisCachingProvider<string[]>, RedisCachingProvider<string[]>>();
        services.AddScoped<IFeatureService, FeatureService>();
    }
}