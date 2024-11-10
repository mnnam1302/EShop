using EShop.Identity.Application.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Identity.Application.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddMediatRApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblies(AssemblyReference.Assembly))
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>))
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformancePipelineBehavior<,>))
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(TracingPipelineBehavior<,>))
                .AddValidatorsFromAssembly(EShop.Shared.Contracts.AssemblyReference.Assembly, includeInternalTypes: true);
    }
}