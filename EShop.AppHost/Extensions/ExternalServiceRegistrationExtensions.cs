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

        // Push approach: OTelCollector + Prometheus
        //var prometheus = builder.AddContainer(ResourceNames.Prometheus, "prom/prometheus", "v3.5.0")
        //    .WithBindMount("../deploy/config/prometheus/prometheus_push.yml", "/etc/prometheus/prometheus.yml", isReadOnly: true)
        //    .WithArgs(
        //        "--web.enable-otlp-receiver",           // enable to expose the endpoint POST /api/v1/otlp.
        //        "--config.file=/etc/prometheus/prometheus.yml",
        //        "--storage.tsdb.retention.time=30d",    // keep 30 days of data
        //        "--storage.tsdb.path=/prometheus"       // explicit data directory
        //    )
        //    .WithHttpEndpoint(targetPort: 9090, name: "http");

        //builder
        //    .AddOpenTelemetryCollector(ResourceNames.OpenTelemetryCollector, @"..\deploy\config\otelcollector\config.yaml")
        //    .WithEnvironment("PROMETHEUS_ENDPOINT", $"{prometheus.GetEndpoint("http")}/api/v1/otlp");

        // Pull approach: OTelCollector + Prometheus
        var otelCollector = builder.AddOpenTelemetryCollector(ResourceNames.OpenTelemetryCollector, @"..\deploy\config\otelcollector\config.yaml");

        var prometheus = builder.AddContainer(ResourceNames.Prometheus, "prom/prometheus", "v3.5.0")
            .WithBindMount("../deploy/config/prometheus/prometheus_pull.yml", "/etc/prometheus/prometheus.yml", isReadOnly: true)
            .WithArgs(
                "--config.file=/etc/prometheus/prometheus.yml",
                "--storage.tsdb.retention.time=30d",
                "--storage.tsdb.path=/prometheus"
            )
            .WithHttpEndpoint(targetPort: 9090, name: "http")
            .WaitFor(otelCollector);

        builder
            .AddContainer("node-exporter", "prom/node-exporter", "v1.8.2")
            .WithBindMount("/proc", "/host/proc", isReadOnly: true)
            .WithBindMount("/sys", "/host/sys", isReadOnly: true)
            .WithBindMount("/", "/rootfs", isReadOnly: true)
            .WithArgs(
                "--path.procfs=/host/proc",
                "--path.rootfs=/rootfs",
                "--path.sysfs=/host/sys",
                "--collector.filesystem.mount-points-exclude=^/(sys|proc|dev|host|etc)($$|/)"
            )
            .WithHttpEndpoint(targetPort: 9100, name: "http")
            .WaitFor(prometheus);

        var grafana = builder.AddContainer(ResourceNames.Grafana, "grafana/grafana")
            .WithBindMount("../deploy/config/grafana/provisioning", "/etc/grafana/provisioning", isReadOnly: true)
            .WithBindMount("../deploy/config/grafana/dashboards", "/var/lib/grafana/dashboards", isReadOnly: true)
            .WithEnvironment("PROMETHEUS_ENDPOINT", prometheus.GetEndpoint("http"))
            .WithHttpEndpoint(targetPort: 3000, name: "http")
            .WaitFor(prometheus);

        #endregion Observability

        #region Infrastructure resources

        var pathToDbInitDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\deploy\Scripts\"));

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
        IResourceBuilder<IResourceWithConnectionString> orderDatabase;
        IResourceBuilder<IResourceWithConnectionString> financeDatabase;

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
            orderDatabase = builder.AddConnectionString("orderDatabase")
                .WithParentRelationship(postgresServer);
            financeDatabase = builder.AddConnectionString("financeDatabase")
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
            orderDatabase = postgres.AddDatabase("orderDatabase", "eshop_order");
            financeDatabase = postgres.AddDatabase("financeDatabase", "eshop_finance");
            catalogMongoDatabase = mongodb.AddDatabase("catalogMongoDatabase", "eshop-catalog");
        }

        #endregion Infrastructure resources

        #region Microservices

        var tenancy = builder.AddProject<Projects.EShop_Tenancy_API>(ResourceNames.TenancyApi)
            .WithExternalServiceMode(useExternalService)
            .WithEnvironment("GRAFANA_URL", grafana.GetEndpoint("http")) // add the same for other microservices
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
            .WithEnvironment("GRAFANA_URL", grafana.GetEndpoint("http"))
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

        //var catalogApplication = builder.AddProject<Projects.EShop_Catalog_Application>(ResourceNames.CatalogWriteApi)
        //    .WithExternalServiceMode(useExternalService)
        //    .WithReference(catalogDatabase)
        //    .WithReference(redis)
        //    .WithReference(rabbitmq);

        //if (!useExternalService)
        //{
        //    catalogApplication
        //        .WaitFor(catalogDatabase)
        //        .WaitFor(redis)
        //        .WaitFor(rabbitmq);
        //}

        //var catalogReadModel = builder.AddProject<Projects.EShop_Catalog_ReadModels_MongoDb>(ResourceNames.CatalogReadApi)
        //    .WithExternalServiceMode(useExternalService)
        //    .WithReference(catalogMongoDatabase)
        //    .WithReference(redis)
        //    .WithReference(rabbitmq);

        //if (!useExternalService)
        //{
        //    catalogReadModel
        //        .WaitFor(catalogMongoDatabase)
        //        .WaitFor(redis)
        //        .WaitFor(rabbitmq)
        //        .WaitFor(catalogApplication);
        //}

        //var inventory = builder.AddProject<Projects.EShop_Inventory_API>(ResourceNames.InventoryApi)
        //    .WithExternalServiceMode(useExternalService)
        //    .WithReference(inventoryDatabase)
        //    .WithReference(redis)
        //    .WithReference(rabbitmq);

        //if (!useExternalService)
        //{
        //    inventory
        //        .WaitFor(inventoryDatabase)
        //        .WaitFor(redis)
        //        .WaitFor(rabbitmq);
        //}

        //var order = builder.AddProject<Projects.EShop_Order_API>(ResourceNames.OrderApi)
        //    .WithExternalServiceMode(useExternalService)
        //    .WithReference(orderDatabase)
        //    .WithReference(redis)
        //    .WithReference(rabbitmq);

        //if (!useExternalService)
        //{
        //    order
        //        .WaitFor(orderDatabase)
        //        .WaitFor(redis)
        //        .WaitFor(rabbitmq);
        //}

        //var finance = builder.AddProject<Projects.EShop_Finance_API>(ResourceNames.FinanceApi)
        //    .WithExternalServiceMode(useExternalService)
        //    .WithReference(financeDatabase)
        //    .WithReference(redis)
        //    .WithReference(rabbitmq);

        //if (!useExternalService)
        //{
        //    finance
        //        .WaitFor(financeDatabase)
        //        .WaitFor(redis)
        //        .WaitFor(rabbitmq);
        //}

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
