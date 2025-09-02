using EShop.Shared.CQRS.Command;
using EShop.Shared.CQRS.Query;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EShop.Shared.CQRS;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCQRS(this IServiceCollection services, Assembly assembly)
    {
        services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        services.AddScoped<IQueryDispatcher, QueryDispatcher>();
        services.AddTransient<IMediator, Mediator>();

        services.Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        services.TryDecorate(typeof(IQueryHandler<,>), typeof(Behaviors.RequestTraicingBehavior.QueryHandler<,>));
        services.TryDecorate(typeof(ICommandHandler<>), typeof(Behaviors.RequestTraicingBehavior.CommandHandler<>));
        services.TryDecorate(typeof(ICommandHandler<,>), typeof(Behaviors.RequestTraicingBehavior.CommandHandler<,>));

        return services;
    }
}
