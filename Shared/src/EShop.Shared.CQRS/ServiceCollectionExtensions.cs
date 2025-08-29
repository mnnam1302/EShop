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
    /// Adds CQRS (Command Query Responsibility Segregation) services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>This method registers the necessary services for implementing CQRS patterns, including
    /// command and query handlers. It scans the provided <paramref name="assembly"/> for types implementing <see
    /// cref="ICommandHandler{TCommand}"/>,  <see cref="ICommandHandler{TCommand, TResult}"/>, and <see
    /// cref="IQueryHandler{TQuery, TResult}"/> interfaces.</remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the CQRS services will be added.</param>
    /// <param name="assembly">The assembly to scan for command and query handler implementations.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddCQRS(this IServiceCollection services, Assembly assembly)
    {
        services.AddCQRS();

        RegisterHandlers(services, assembly, typeof(ICommandHandler<>));
        RegisterHandlers(services, assembly, typeof(ICommandHandler<,>));
        RegisterHandlers(services, assembly, typeof(IQueryHandler<,>));

        return services;
    }

    public static IServiceCollection AddCQRS(this IServiceCollection services)
    {
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        services.AddTransient<IMediator, Mediator>();

        //services.TryAddScoped<CommandDispatcher>();
        //services.TryAddScoped<QueryDispatcher>();
        //services.TryAddTransient<Mediator>();

        services.TryAddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        return services;
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
