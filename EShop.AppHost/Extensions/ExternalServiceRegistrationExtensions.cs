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

        IResourceBuilder<IResourceWithConnectionString> tenancyDatabase;
        IResourceBuilder<IResourceWithConnectionString> authorizationDatabase;
        IResourceBuilder<IResourceWithConnectionString> catalogDatabase;
        IResourceBuilder<IResourceWithConnectionString> catalogMongoDatabase;
        IResourceBuilder<IResourceWithConnectionString> inventoryDatabase;

        if (useExternalService)
        {
            var postgresServer = builder.AddConnectionString(ResourceNames.PostgreSql);
            var mongoServer = builder.AddConnectionString(ResourceNames.MongoDb);

            tenancyDatabase = builder.AddConnectionString("tenancyDatabase")
                .WithParentRelationship(postgresServer);
            authorizationDatabase = builder.AddConnectionString("authorizationDatabase")
                .WithParentRelationship(postgresServer);
            catalogDatabase = builder.AddConnectionString("catalogDatabase")
                .WithParentRelationship(postgresServer);
            inventoryDatabase = builder.AddConnectionString("inventoryDatabase")
                .WithParentRelationship(postgresServer);

            catalogMongoDatabase = builder.AddConnectionString("catalogMongoDatabase")
                .WithParentRelationship(mongoServer);
        }
        else
        {
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

            var mongodb = builder
                .AddMongoDB(ResourceNames.MongoDb)
                .WithImageTag("6.0")
                .WithDataVolume("eshop-mongodb-data")
                .WithLifetime(ContainerLifetime.Persistent);

            tenancyDatabase = postgres.AddDatabase("tenancyDatabase", "eshop_tenancy");
            authorizationDatabase = postgres.AddDatabase("authorizationDatabase", "eshop_authorization");
            catalogDatabase = postgres.AddDatabase("catalogDatabase", "eshop_catalog");
            inventoryDatabase = postgres.AddDatabase("inventoryDatabase", "eshop_inventory");
            catalogMongoDatabase = mongodb.AddDatabase("catalogMongoDatabase", "eshop-catalog");
        }

        #endregion Infrastructure resources

        #region Microservices

        var tenancy = builder.AddProject<Projects.EShop_Tenancy_API>(ResourceNames.TenancyApi)
            .WithExternalServiceMode(useExternalService)
            //.WithEnvironment("GRAFANA_URL", grafana.GetEndpoint("http")) // add the same for other microservices
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

        var authorization = builder.AddProject<Projects.EShop_Authorization_API>(ResourceNames.AuthorizationApi)
            .WithExternalServiceMode(useExternalService)
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

        var catalogApplication = builder.AddProject<Projects.EShop_Catalog_Application>(ResourceNames.CatalogWriteApi)
            .WithExternalServiceMode(useExternalService)
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

        var catalogReadModel = builder.AddProject<Projects.EShop_Catalog_ReadModels_MongoDb>(ResourceNames.CatalogReadApi)
            .WithExternalServiceMode(useExternalService)
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

        var inventory = builder.AddProject<Projects.EShop_Inventory_API>(ResourceNames.InventoryApi)
            .WithExternalServiceMode(useExternalService)
            .WithReference(inventoryDatabase)
            .WithReference(redis)
            .WithReference(rabbitmq);

        if (!useExternalService)
        {
            inventory
                .WaitFor(inventoryDatabase)
                .WaitFor(redis)
                .WaitFor(rabbitmq);
        }

        //builder.AddProject<Projects.EShop_Order_API>("eshop-order-api");

        #endregion Microservices

        #region Api Gateway

        //var apiGateway = builder.AddProject<Projects.EShop_ApiGateway>(ResourceNames.ApiGateway)
        //    .WithReference(redis)
        //    .WithReference(tenancy)
        //    .WithReference(authorization)
        //    .WithReference(catalogApplication)
        //    .WithReference(catalogReadModel);

        //if (!useExternalService)
        //{
        //    apiGateway
        //        .WaitFor(redis)
        //        .WaitFor(tenancy)
        //        .WaitFor(authorization)
        //        .WaitFor(catalogApplication)
        //        .WaitFor(catalogReadModel);
        //}

        #endregion Api Gateway

        return builder;
    }

    public static IResourceBuilder<T> WithExternalServiceMode<T>(this IResourceBuilder<T> builder, bool isExternal) where T : IResourceWithEnvironment
    {
        return builder.WithEnvironment("IsExternalServiceMode", isExternal.ToString());
    }
}
