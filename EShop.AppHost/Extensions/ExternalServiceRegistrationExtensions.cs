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
        #region Observability
        var prometheus = builder.AddContainer(ResourceNames.Prometheus, "prom/prometheus", "v3.5.0")
                                .WithBindMount("../Deployment/config/prometheus/prometheus.yml", "/etc/prometheus/prometheus.yml")
                                .WithArgs("--web.enable-otlp-receiver", "--config.file=/etc/prometheus/prometheus.yml")
                                .WithHttpEndpoint(targetPort: 9090, name: "http");

        var grafana = builder.AddContainer("grafana", "grafana/grafana")
                             .WithBindMount("../Deployment/config/grafana/config", "/etc/grafana", isReadOnly: true)
                             .WithBindMount("../Deployment/config/grafana/dashboards", "/var/lib/grafana/dashboards", isReadOnly: true)
                             .WithEnvironment("PROMETHEUS_ENDPOINT", prometheus.GetEndpoint("http"))
                             .WithHttpEndpoint(targetPort: 3000, name: "http");

        builder.AddOpenTelemetryCollector(ResourceNames.OpenTelemetryCollector, @"..\Deployment\config\otelcollector\config.yaml")
               .WithEnvironment("PROMETHEUS_ENDPOINT", $"{prometheus.GetEndpoint("http")}/api/v1/otlp");

        #endregion

        #region Infrastructure resources
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

        #endregion

        #region Microservices
        var tenancyDatabase = postgres.AddDatabase("tenancyDatabase", "eshop_tenancy");
        var tenancy = builder.AddProject<Projects.EShop_Tenancy_API>(ResourceNames.TenancyApi)
            .WithExternalServiceMode(useExternalService)
            .WithEnvironment("GRAFANA_URL", grafana.GetEndpoint("http"))
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

        var authorizationDatabase = postgres.AddDatabase("authorizationDatabase", "eshop_authorization");
        var authrorization = builder.AddProject<Projects.EShop_Authorization_API>(ResourceNames.AuthorizationApi)
            .WithExternalServiceMode(useExternalService)
            .WithEnvironment("GRAFANA_URL", grafana.GetEndpoint("http"))
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

        #endregion

        return builder;
    }

    public static IResourceBuilder<T> WithExternalServiceMode<T>(this IResourceBuilder<T> builder, bool isExternal) where T : IResourceWithEnvironment
    {
        return builder.WithEnvironment("IsExternalServiceMode", isExternal.ToString());
    }
}