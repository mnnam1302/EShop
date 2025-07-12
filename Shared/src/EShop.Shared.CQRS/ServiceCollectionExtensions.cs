using EShop.Shared.CQRS.Command;
using EShop.Shared.CQRS.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;

namespace EShop.Shared.CQRS;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds CQRS infrastructure services to the service collection
    /// </summary>
    public static IServiceCollection AddCQRS(this IServiceCollection services)
    {
        // Register interfaces
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        services.AddTransient<IMediator, Mediator>();

        // Register concrete implementations (needed for some tests)
        services.TryAddScoped<CommandDispatcher>();
        services.TryAddScoped<QueryDispatcher>();
        services.TryAddTransient<Mediator>();

        // Ensure logging is available (fallback to null logger if not registered)
        services.TryAddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        return services;
    }

    /// <summary>
    /// Adds CQRS infrastructure services and automatically registers handlers from the specified assembly
    /// </summary>
    public static IServiceCollection AddCQRS(this IServiceCollection services, Assembly assembly)
    {
        services.AddCQRS();

        RegisterHandlers(services, assembly, typeof(ICommandHandler<>));
        RegisterHandlers(services, assembly, typeof(ICommandHandler<,>));
        RegisterHandlers(services, assembly, typeof(IQueryHandler<,>));

        return services;
    }

    /// <summary>
    /// Legacy method for backward compatibility
    /// </summary>
    public static IServiceCollection AddCqrs(this IServiceCollection services, Assembly assembly)
    {
        return services.AddCQRS(assembly);
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly assembly, Type handlerInterfaceType)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType));

        foreach (var handlerType in handlerTypes)
        {
            var interfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType);

            foreach (var @interface in interfaces)
            {
                services.TryAddScoped(@interface, handlerType);
            }
        }
    }
}
