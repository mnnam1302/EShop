using EShop.Shared.JsonApi.Behaviors;
using EShop.Tenancy.Application.DependencyInjections.Extensions;
using EShop.Tenancy.Application.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Tenancy.Application.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTenancyApplication(this IServiceCollection services)
    {
        services
            .AddMediatR()
            .AddAutoMapperService();

        services.AddServices();

        return services;
    }

    private static IServiceCollection AddMediatR(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblies(AssemblyReference.Assembly))
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>))
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformancePipelineBehavior<,>))
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(TracingPipelineBehavior<,>))
                .AddValidatorsFromAssembly(Shared.Contracts.AssemblyReference.Assembly, includeInternalTypes: true);
        
        return services;
    }

    private static IServiceCollection AddAutoMapperService(this IServiceCollection services)
    {
        services.AddAutoMapper(AssemblyReference.Assembly);
        return services;
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IFeatureService, FeatureService>();
    }
}