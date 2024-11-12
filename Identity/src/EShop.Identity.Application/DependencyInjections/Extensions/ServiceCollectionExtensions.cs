using EShop.Identity.Application.Behaviors;
using EShop.Identity.Application.Services;
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

    public static void AddAutoMapperApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(AssemblyReference.Assembly);
    }

    public static void AddServicesApplication(this IServiceCollection services)
    {
        services.AddTransient<IPermissionCalculator, PermissionCalculator>(); // Interface inside service users
        //services.AddTransient<IUserPermissionsProvider, PermissionCalculator>(); // Interface inside Shared.Scoping
    }
}