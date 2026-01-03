using EShop.AppHost.OpenTelemetryCollector;

namespace EShop.AppHost.Extensions;

public static class ExternalServiceRegistrationExtensions
{
    public static IDistributedApplicationBuilder AddExternalServices(this IDistributedApplicationBuilder builder)
    {
        return AddServices(builder, true);
    }

    public static IDistributedApplicationBuilder AddServiceDefaults(this IDistributedApplicationBuilder builder)
    {
        return AddServices(builder, false);
    }

    private static IDistributedApplicationBuilder AddServices(IDistributedApplicationBuilder builder, bool useExternalService)
    {
        // Observability
        builder.AddOpenTelemetryCollector(ResourceNames.OpenTelemetryCollector, @"..\Deploment\config\otelcollector\config.yaml");

        // Infrastructure resources
        var pathToDbInitDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\Deployment\Scripts\"));
        var postgres = builder.AddPostgres(ResourceNames.PostgreSql, port: 5432)
                .WithImageTag("17.0")
                .WithDataVolume("ehop-data")
                .WithInitFiles(pathToDbInitDirectory)
                .WithArgs("-c", "max_connections=200")
                .WithPgAdmin(rb =>
                {
                    rb.WithLifetime(ContainerLifetime.Persistent);
                    rb.WithHostPort(5442);
                })
                .WithLifetime(ContainerLifetime.Persistent);

        var redis = useExternalService
            ? builder.AddConnectionString(ResourceNames.Redis)
            : builder
                .AddRedis(ResourceNames.Redis)
                .WithDataVolume("eshop-redis-data")
                .WithRedisInsight()
                .WithLifetime(ContainerLifetime.Persistent);

        var rabbitmq = useExternalService
            ? builder.AddConnectionString(ResourceNames.RabbitMq)
            : builder
                .AddRabbitMQ(ResourceNames.RabbitMq)
                .WithDataVolume("eshop-rabbitmq-data")
                .WithLifetime(ContainerLifetime.Persistent)
                .WithManagementPlugin();

        // Tenancy Microservice
        var tenancyDatabase = postgres.AddDatabase("tenancyDatabase", "eshop_tenancy");
        var tenancy = builder.AddProject<Projects.EShop_Tenancy_API>(ResourceNames.TenancyApi)
            .WithExternalServiceMode(useExternalService)
            .WithReference(tenancyDatabase)
            .WithReference(redis)
            .WithReference(rabbitmq);

        if (!useExternalService)
        {
            tenancy
                .WaitFor(tenancyDatabase)
                .WaitFor(redis)
                .WaitFor(rabbitmq);
        }

        // Authorization Microservice
        var authorizationDatabase = postgres.AddDatabase("authorizationDatabase", "eshop_authorization");
        var authrorization = builder.AddProject<Projects.EShop_Authorization_API>(ResourceNames.AuthorizationApi)
            .WithExternalServiceMode(useExternalService)
            .WithReference(authorizationDatabase)
            .WithReference(redis)
            .WithReference(rabbitmq);

        if (!useExternalService)
        {
            authrorization
                .WaitFor(authorizationDatabase)
                .WaitFor(redis)
                .WaitFor(rabbitmq);
        }

        return builder;
    }

    public static IResourceBuilder<T> WithExternalServiceMode<T>(this IResourceBuilder<T> builder, bool isExternal) where T : IResourceWithEnvironment
    {
        return builder.WithEnvironment("IsExternalServiceMode", isExternal.ToString());
    }
}