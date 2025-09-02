using EShop.Shared.CQRS.Command;
using EShop.Shared.CQRS.Query;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace EShop.Shared.CQRS;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCQRS(this IServiceCollection services, Assembly assembly)
    {
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        services.AddScoped<IMediator, Mediator>();

        var handlerInterfaceTypes = new[]
         {
            typeof(ICommandHandler<>),
            typeof(ICommandHandler<,>),
            typeof(IQueryHandler<,>)
        };

        foreach (var handlerInterfaceType in handlerInterfaceTypes)
        {
            RegisterGenericHandlers(services, assembly, handlerInterfaceType);
        }

        return services;
    }

    private static void RegisterGenericHandlers(IServiceCollection services, Assembly assembly, Type handlerInterfaceType)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType));

        foreach (var handlerType in handlerTypes)
        {
            var implementedInterfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType);

            foreach (var @interface in implementedInterfaces)
            {
                services.TryAddScoped(@interface, handlerType);
            }
        }
    }
}
