using System.Reflection;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using MassTransit;

namespace EShop.Shared.EventBus.DependencyInjections.Extensions;

public static class RabbitMqBusFactoryConfiguratorExtensions
{
    /// <summary>
    /// Scans an assembly for all concrete <see cref="IConsumer{T}"/> implementations
    /// and configures a receive endpoint for each consumer/event pair found.
    /// Follows the same queue naming convention as
    /// <see cref="ConfigureReceiveEndpoint{TConsumer,TEvent}"/>.
    /// </summary>
    /// <param name="bus">The RabbitMQ bus configurator.</param>
    /// <param name="context">The MassTransit registration context.</param>
    /// <param name="environment">The hosting environment name (e.g. "Development").</param>
    /// <param name="serviceName">The application/service name for queue naming.</param>
    /// <param name="assembly">The assembly to scan for consumer types.</param>
    public static void ConfigureEventReceiveEndpoints(
        this IRabbitMqBusFactoryConfigurator bus,
        IRegistrationContext context,
        string environment,
        string serviceName,
        Assembly assembly)
    {
        var configureMethod = typeof(RabbitMqBusFactoryConfiguratorExtensions)
            .GetMethod(nameof(ConfigureReceiveEndpoint))!;

        foreach (var consumerType in assembly.GetTypes().Where(t => t is { IsAbstract: false, IsClass: true }))
        {
            var consumerInterface = consumerType
                .GetInterfaces()
                .FirstOrDefault(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IConsumer<>) &&
                    typeof(IEvent).IsAssignableFrom(i.GenericTypeArguments[0]));

            if (consumerInterface is null)
            {
                continue;
            }

            var eventType = consumerInterface.GenericTypeArguments[0];

            configureMethod.MakeGenericMethod(consumerType, eventType)
                .Invoke(null, [bus, context, environment, serviceName]);
        }
    }

    public static void ConfigureReceiveEndpoint<TConsumer, TEvent>(
        this IRabbitMqBusFactoryConfigurator bus,
        IRegistrationContext context,
        string environment,
        string serviceName)
        where TConsumer : class, IConsumer<TEvent>
        where TEvent : class, IEvent
    {
        string normalizedEnvironment = NormalizeEnvironmentName(environment);
        string normalizedServiceName = serviceName.Trim().ToLowerInvariant();

        string sanitizedQueueName = $"eshop.{normalizedEnvironment}.{normalizedServiceName}.{typeof(TEvent).ToKebabCaseString()}";

        bus.ReceiveEndpoint(
            queueName: sanitizedQueueName,
            configureEndpoint: endpoint =>
            {
                endpoint.ConfigureConsumeTopology = false;
                endpoint.ConfigureConsumer<TConsumer>(context);
                endpoint.Bind<TEvent>();
            });
    }

    private static string NormalizeEnvironmentName(string environment)
    {
        return environment.Trim().ToLowerInvariant() switch
        {
            "development" => "dev",
            "staging" => "stag",
            "production" => "prod",
            _ => string.Empty,
        };
    }
}
