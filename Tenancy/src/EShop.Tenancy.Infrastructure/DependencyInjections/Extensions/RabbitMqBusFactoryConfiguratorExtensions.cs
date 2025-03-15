using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Tenancy.Infrastructure.Consumers;
using MassTransit;
using Microsoft.AspNetCore.Hosting;

namespace EShop.Tenancy.Infrastructure.DependencyInjections.Extensions;

public static class RabbitMqBusFactoryConfiguratorExtensions
{
    public static void ConfigureRecieveEndpoints(this IRabbitMqBusFactoryConfigurator bus, IRegistrationContext context, IWebHostEnvironment environment)
    {
        bus.ConfigureRecieveEndpoints<FeatureEventConsumers, SupportedFeaturesUpdated>(context, environment);
    }

    private static void ConfigureRecieveEndpoints<TConsumer, TEvent>(
        this IRabbitMqBusFactoryConfigurator bus,
        IRegistrationContext context,
        IWebHostEnvironment environment)
        where TConsumer : class, IConsumer
        where TEvent : class, IEvent
    {
        bus.ReceiveEndpoint(
            queueName: $"{environment.EnvironmentName}.{typeof(TConsumer).ToKebabCaseString()}.{typeof(TEvent).ToKebabCaseString()}",
            configureEndpoint: endpoint =>
            {
                endpoint.ConfigureConsumeTopology = false;
                endpoint.Bind<TEvent>();
                endpoint.ConfigureConsumer<TConsumer>(context);
            });
    }
}