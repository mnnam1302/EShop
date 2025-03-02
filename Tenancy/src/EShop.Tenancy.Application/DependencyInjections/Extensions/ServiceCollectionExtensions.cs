using EShop.Shared.JsonApi.Behaviors;
using EShop.Tenancy.Application.DependencyInjections.Extensions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Tenancy.Application.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTenancyApplication(this IServiceCollection services)
    {
        services.AddMediatR();
        services.AddAutoMapperService();

        return services;
    }

    private static void AddMediatR(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblies(AssemblyReference.Assembly))
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>))
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformancePipelineBehavior<,>))
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(TracingPipelineBehavior<,>))
                .AddValidatorsFromAssembly(Shared.Contracts.AssemblyReference.Assembly, includeInternalTypes: true);
    }

    private static void AddAutoMapperService(this IServiceCollection services)
    {
        services.AddAutoMapper(AssemblyReference.Assembly);
    }
}