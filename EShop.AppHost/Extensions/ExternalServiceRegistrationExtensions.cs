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

        //var prometheus = builder.AddContainer(ResourceNames.Prometheus, "prom/prometheus", "v3.5.0")
        //    .WithBindMount("../Deployment/config/prometheus/prometheus.yml", "/etc/prometheus/prometheus.yml", isReadOnly: true)
        //    .WithArgs("--web.enable-otlp-receiver", "--config.file=/etc/prometheus/prometheus.yml")
        //    .WithHttpEndpoint(targetPort: 9090, name: "http");

        //var grafana = builder.AddContainer(ResourceNames.Grafana, "grafana/grafana")
        //    .WithBindMount("../Deployment/config/grafana/config", "/etc/grafana", isReadOnly: true)
        //    .WithBindMount("../Deployment/config/grafana/dashboards", "/var/lib/grafana/dashboards", isReadOnly: true)
        //    .WithEnvironment("PROMETHEUS_ENDPOINT", prometheus.GetEndpoint("http"))
        //    .WithHttpEndpoint(targetPort: 3000, name: "http");

        //builder.AddOpenTelemetryCollector(ResourceNames.OpenTelemetryCollector, @"..\Deployment\config\otelcollector\config.yaml")
        //       .WithEnvironment("PROMETHEUS_ENDPOINT", $"{prometheus.GetEndpoint("http")}/api/v1/otlp");

        #endregion Observability

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
                .WithImageTag("7.4.7")
                .WithDataVolume("eshop-redis-data")
                .WithRedisInsight()
                .WithLifetime(ContainerLifetime.Persistent);

        var rabbitmq = useExternalService
            ? builder.AddConnectionString(ResourceNames.RabbitMq)
            : builder
                .AddRabbitMQ(ResourceNames.RabbitMq)
                .WithImageTag("4.1")
                .WithDataVolume("eshop-rabbitmq-data")
                .WithLifetime(ContainerLifetime.Persistent)
                .WithManagementPlugin();

        var mongodb = builder
            .AddMongoDB(ResourceNames.MongoDb)
            .WithImageTag("6.0")
            .WithDataVolume("eshop-mongodb-data")
            .WithLifetime(ContainerLifetime.Persistent);

        var catalogMongoDatabase = useExternalService
            ? builder.AddConnectionString("catalogMongoDatabase")
            : mongodb.AddDatabase("catalogMongoDatabase", "eshop-catalog");

        #endregion Infrastructure resources

        #region Microservices

        var tenancyDatabase = postgres.AddDatabase("tenancyDatabase", "eshop_tenancy");
        var tenancy = builder.AddProject<Projects.EShop_Tenancy_API>(ResourceNames.TenancyApi)
            .WithExternalServiceMode(useExternalService)
            //.WithEnvironment("GRAFANA_URL", grafana.GetEndpoint("http"))
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
        var authorization = builder.AddProject<Projects.EShop_Authorization_API>(ResourceNames.AuthorizationApi)
            .WithExternalServiceMode(useExternalService)
            //.WithEnvironment("GRAFANA_URL", grafana.GetEndpoint("http"))
            .WithReference(authorizationDatabase)
            .WithReference(redis)
            .WithReference(rabbitmq);

        if (!useExternalService)
        {
            authorization
                .WaitFor(authorizationDatabase)
                .WaitFor(redis)
                .WaitFor(rabbitmq);
        }

        var catalogDatabase = postgres.AddDatabase("catalogDatabase", "eshop_catalog");
        var catalogApplication = builder.AddProject<Projects.EShop_Catalog_Application>("catalog-application")
            .WithExternalServiceMode(useExternalService)
            //.WithEnvironment("GRAFANA_URL", grafana.GetEndpoint("http"))
            .WithReference(catalogDatabase)
            .WithReference(redis)
            .WithReference(rabbitmq);

        if (!useExternalService)
        {
            catalogApplication
                .WaitFor(catalogDatabase)
                .WaitFor(redis)
                .WaitFor(rabbitmq);
        }

        var catalogReadModel = builder.AddProject<Projects.EShop_Catalog_ReadModels_MongoDb>("catalog-readmodel")
            .WithExternalServiceMode(useExternalService)
            //.WithEnvironment("GRAFANA_URL", grafana.GetEndpoint("http"))
            .WithReference(catalogMongoDatabase)
            .WithReference(redis)
            .WithReference(rabbitmq);

        if (!useExternalService)
        {
            catalogReadModel
                .WaitFor(catalogMongoDatabase)
                .WaitFor(redis)
                .WaitFor(rabbitmq)
                .WaitFor(catalogApplication);
        }

        #endregion Microservices

        #region Api Gateway

        var apiGateway = builder.AddProject<Projects.EShop_ApiGateway>(ResourceNames.ApiGateway)
            .WithReference(redis)
            .WithReference(tenancy)
            .WithReference(authorization)
            .WithReference(catalogApplication)
            .WithReference(catalogReadModel);

        if (!useExternalService)
        {
            apiGateway
                .WaitFor(redis)
                .WaitFor(tenancy)
                .WaitFor(authorization)
                .WaitFor(catalogApplication)
                .WaitFor(catalogReadModel);
        }

        #endregion Api Gateway

        return builder;
    }

    public static IResourceBuilder<T> WithExternalServiceMode<T>(this IResourceBuilder<T> builder, bool isExternal) where T : IResourceWithEnvironment
    {
        return builder.WithEnvironment("IsExternalServiceMode", isExternal.ToString());
    }
}