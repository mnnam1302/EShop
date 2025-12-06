using EShop.Authorization.Application.Abstractions;
using EShop.Authorization.Domain.Repositories;
using EShop.Authorization.Infrastructure.Consumers;
using EShop.Authorization.Infrastructure.EmailServices;
using EShop.Authorization.Infrastructure.Producers;
using EShop.Authorization.Infrastructure.Repositories;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Contracts.IntegrationEvents.Authorization;
using EShop.Shared.Contracts.JsonConverters;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.EventBus.Abstractions;
using EShop.Shared.EventBus.DependencyInjections.Extensions;
using EShop.Shared.EventBus.DependencyInjections.Options;
using EShop.Shared.EventBus.PipelineObservers;
using EShop.Shared.EventBus.Services;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.Scoping.ResourceAccessControl;
using FluentEmail.MailKitSmtp;
using MailKit.Security;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Authorization.Infrastructure.DependencyInjections;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthorizationPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgreSqlHealthCheck(configuration)
            .AddDbContextWithScoping<AuthorizationDbContext>(configuration)
            .AddPersistenceServices();

        services.AddMemoryCacheInfrastructure();

        return services;
    }

    public static void AddPersistenceServices(this IServiceCollection services)
    {
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();

        services.AddScoped<IUnitOfWork, EFUnitOfWork<AuthorizationDbContext>>();
    }

    public static IServiceCollection AddAuthorizationInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddEventBus()
            .AddMasstransitRabbitMQ(configuration, environment)
            .AddProducers();

        services.AddRedisHealthCheck(configuration)
            .AddRedisCacheInfrastructure(configuration);

        services.AddEmailServices(configuration);

        return services;
    }

    public static IServiceCollection AddEventBus(this IServiceCollection services)
    {
        services.AddScoped<IEventBusGateway, EventBusGateway>();
        return services;
    }

    private static IServiceCollection AddProducers(this IServiceCollection services)
    {
        services.AddTransient<IFeatureRegistrationService, AuthorizationFeatureRegistrationProducer>();
        services.AddTransient<IPermissionRegistrationService, AuthorizationPermissionRegistrationProducer>();

        return services;
    }

    private static IServiceCollection AddMasstransitRabbitMQ(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var massTransitConfiguration = new MasstransitConfiguration();
        configuration.GetSection(nameof(MasstransitConfiguration)).Bind(massTransitConfiguration);

        var messageBusOptions = new MessageBusOptions();
        configuration.GetSection(nameof(MessageBusOptions)).Bind(messageBusOptions);

        services.AddMassTransit(cfg =>
        {
            cfg.SetKebabCaseEndpointNameFormatter();

            cfg.AddConsumers(AssemblyReference.Assembly);

            cfg.UsingRabbitMq((context, bus) =>
            {
                bus.Host(massTransitConfiguration.Host, massTransitConfiguration.Port, massTransitConfiguration.VHost, h =>
                {
                    h.Username(massTransitConfiguration.Username);
                    h.Password(massTransitConfiguration.Password);
                });

                bus.UseMessageRetry(retry
                    => retry.Incremental(
                            retryLimit: messageBusOptions.RetryLimit,
                            initialInterval: messageBusOptions.InitialInterval,
                            intervalIncrement: messageBusOptions.IntervalIncrement));

                bus.UseNewtonsoftJsonSerializer();
                bus.ConfigureNewtonsoftJsonSerializer(settings =>
                {
                    settings.Converters.Add(new DateOnlyJsonConverter());
                    settings.Converters.Add(new ExpirationDateOnlyJsonConverter());
                    return settings;
                });
                bus.ConfigureNewtonsoftJsonDeserializer(settings =>
                {
                    settings.Converters.Add(new DateOnlyJsonConverter());
                    settings.Converters.Add(new ExpirationDateOnlyJsonConverter());
                    return settings;
                });

                bus.ConnectPublishObserver(new LoggingPublishObserver());
                bus.ConnectSendObserver(new LoggingSendObserver());
                bus.ConnectReceiveObserver(new LoggingReceiveObserver());
                bus.ConnectConsumeObserver(new LoggingConsumeObserver());

                bus.MessageTopology.SetEntityNameFormatter(new KebabCaseEntityNameFormatter());

                bus.ConfigureReceiveEndpoints(context, environment, "Authorization");
                bus.ConfigureEndpoints(context);
            });
        });

        return services;
    }

    private static void ConfigureReceiveEndpoints(
        this IRabbitMqBusFactoryConfigurator bus,
        IRegistrationContext context,
        IWebHostEnvironment environment,
        string serviceName)
    {
        bus.ConfigureEventReceiveEndpoint<TenantCreatedConsumer, ITenantCreated>(
            context,
            environment.EnvironmentName,
            serviceName);
        bus.ConfigureEventReceiveEndpoint<SupportedPermissionsUpdatedConsumer, SupportedPermissionsUpdated>(
            context,
            environment.EnvironmentName,
            serviceName);
    }

    public static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<EmailSettingOptions>()
            .BindConfiguration(EmailSettingOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var emailSettings = configuration
            .GetSection(EmailSettingOptions.SectionName)
            .Get<EmailSettingOptions>()
            ?? throw new InvalidOperationException("Email settings are not configured properly.");

        services.AddFluentEmail(emailSettings.DefaultFromEmail)
            .AddMailKitSender(new SmtpClientOptions
            {
                Server = emailSettings.Host,
                Port = emailSettings.Port,
                User = emailSettings.UserName,
                Password = emailSettings.Password,
                UseSsl = emailSettings.EnableSsl,
                RequiresAuthentication = true,
                SocketOptions = SecureSocketOptions.StartTlsWhenAvailable
            });

        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}