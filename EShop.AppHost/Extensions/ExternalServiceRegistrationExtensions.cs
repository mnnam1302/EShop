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

        IResourceBuilder<IResourceWithEndpoints>? grafana = null;

        if (!useExternalService)
        {
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

            // Pull approach: OTelCollector + Prometheus + Grafana (Chỉ chạy ở Local)
            var otelCollector = builder.AddOpenTelemetryCollector(ResourceNames.OpenTelemetryCollector, @"..\deploy\config\otelcollector\config.yaml");

            var prometheus = builder.AddContainer(ResourceNames.Prometheus, "prom/prometheus", "v3.5.0")
                .WithBindMount("../deploy/config/prometheus/prometheus_pull.yml", "/etc/prometheus/prometheus.yml", isReadOnly: true)
                .WithArgs("--config.file=/etc/prometheus/prometheus.yml", "--storage.tsdb.retention.time=30d", "--storage.tsdb.path=/prometheus")
                .WithHttpEndpoint(targetPort: 9090, name: "http")
                .WaitFor(otelCollector);

            builder.AddContainer("node-exporter", "prom/node-exporter", "v1.8.2")
                .WithBindMount("/proc", "/host/proc", isReadOnly: true)
                .WithBindMount("/sys", "/host/sys", isReadOnly: true)
                .WithBindMount("/", "/rootfs", isReadOnly: true)
                .WithArgs(
                    "--path.procfs=/host/proc",
                    "--path.rootfs=/rootfs",
                    "--path.sysfs=/host/sys",
                    "--collector.filesystem.mount-points-exclude=^/(sys|proc|dev|host|etc)($|/)"
                )
                .WithHttpEndpoint(targetPort: 9100, name: "http")
                .WaitFor(prometheus);

            grafana = builder.AddContainer(ResourceNames.Grafana, "grafana/grafana")
                .WithBindMount("../deploy/config/grafana/provisioning", "/etc/grafana/provisioning", isReadOnly: true)
                .WithBindMount("../deploy/config/grafana/dashboards", "/var/lib/grafana/dashboards", isReadOnly: true)
                .WithEnvironment("PROMETHEUS_ENDPOINT", prometheus.GetEndpoint("http"))
                .WithHttpEndpoint(targetPort: 3000, name: "http")
                .WaitFor(prometheus);
        }

        #endregion Observability

        #region Infrastructure resources

        var redis = useExternalService
            ? builder.AddConnectionString(ResourceNames.Redis)
            : builder
                .AddRedis(ResourceNames.Redis)
                .WithImageTag("7.4.7")
                .WithDataVolume("eshop-redis")
                .WithRedisInsight()
                .WithLifetime(ContainerLifetime.Persistent);

        var rabbitmq = useExternalService
            ? builder.AddConnectionString(ResourceNames.RabbitMq)
            : builder
                .AddRabbitMQ(ResourceNames.RabbitMq)
                .WithImageTag("3")
                .WithDataVolume("eshop-rabbitmq")
                .WithLifetime(ContainerLifetime.Persistent)
                .WithManagementPlugin();

        #endregion Infrastructure resources

        if (useExternalService)
        {
            var postgres = builder.AddConnectionString(ResourceNames.PostgreSql);

            var tenancyDatabase = builder.AddConnectionString("tenancyDatabase").WithParentRelationship(postgres);
            var authorizationDatabase = builder.AddConnectionString("authorizationDatabase").WithParentRelationship(postgres);

            var catalogWriteDatabase = builder.AddConnectionString("catalogDatabase").WithParentRelationship(postgres);
            var mongoDb = builder.AddConnectionString(ResourceNames.MongoDb);
            var catalogReadDatabase = builder.AddConnectionString("catalogMongoDatabase").WithParentRelationship(mongoDb);

            var inventoryDatabase = builder.AddConnectionString("inventoryDatabase").WithParentRelationship(postgres);
            var orderDatabase = builder.AddConnectionString("orderDatabase").WithParentRelationship(postgres);
            var financeDatabase = builder.AddConnectionString("financeDatabase").WithParentRelationship(postgres);

            RegisterMicroservices(
                builder,
                useExternalService,
                redis,
                rabbitmq,
                tenancyDatabase,
                authorizationDatabase,
                catalogWriteDatabase,
                catalogReadDatabase,
                inventoryDatabase,
                orderDatabase,
                financeDatabase,
                grafana: null);
        }
        else
        {
            var mongodbContainer = builder.AddMongoDB(ResourceNames.MongoDb)
                .WithImageTag("6.0")
                .WithDataVolume("eshop-mongodb")
                .WithLifetime(ContainerLifetime.Persistent);

            var pathToDbInitDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\deploy\postgres\init-scripts\"));

            var postgresContainer = builder.AddPostgres(ResourceNames.PostgreSql, port: 5432)
                .WithImageTag("17.0")
                .WithDataVolume("eshop-data")
                .WithInitFiles(pathToDbInitDirectory)
                .WithArgs("-c", "max_connections=200")
                .WithLifetime(ContainerLifetime.Persistent);

            var tenancyDatabase = postgresContainer.AddDatabase("tenancyDatabase", "eshop_tenancy");
            var authorizationDatabase = postgresContainer.AddDatabase("authorizationDatabase", "eshop_authorization");

            var catalogWriteDatabase = postgresContainer.AddDatabase("catalogDatabase", "eshop_catalog");
            var catalogReadDatabase = mongodbContainer.AddDatabase("catalogMongoDatabase", "eshop-catalog");

            var inventoryDatabase = postgresContainer.AddDatabase("inventoryDatabase", "eshop_inventory");
            var orderDatabase = postgresContainer.AddDatabase("orderDatabase", "eshop_order");
            var financeDatabase = postgresContainer.AddDatabase("financeDatabase", "eshop_finance");

            RegisterMicroservices(
                builder,
                useExternalService,
                redis,
                rabbitmq,
                tenancyDatabase,
                authorizationDatabase,
                catalogWriteDatabase,
                catalogReadDatabase,
                inventoryDatabase,
                orderDatabase,
                financeDatabase,
                grafana: grafana);
        }

        return builder;
    }

    private static void RegisterMicroservices(
        IDistributedApplicationBuilder builder,
        bool useExternalService,
        IResourceBuilder<IResourceWithConnectionString> redis,
        IResourceBuilder<IResourceWithConnectionString> rabbitmq,
        IResourceBuilder<IResourceWithConnectionString> tenancyDatabase,
        IResourceBuilder<IResourceWithConnectionString> authorizationDatabase,
        IResourceBuilder<IResourceWithConnectionString> catalogDatabase,
        IResourceBuilder<IResourceWithConnectionString> catalogMongoDatabase,
        IResourceBuilder<IResourceWithConnectionString> inventoryDatabase,
        IResourceBuilder<IResourceWithConnectionString> orderDatabase,
        IResourceBuilder<IResourceWithConnectionString> financeDatabase,
        IResourceBuilder<IResourceWithEndpoints>? grafana)
    {
        #region Microservices

        var tenancy = builder.AddProject<Projects.EShop_Tenancy_API>(ResourceNames.TenancyApi)
            .WithExternalServiceMode(useExternalService)
            .WithReference(tenancyDatabase)
            .WithReference(redis)
            .WithReference(rabbitmq);

        var authorization = builder.AddProject<Projects.EShop_Authorization_API>(ResourceNames.AuthorizationApi)
            .WithExternalServiceMode(useExternalService)
            .WithReference(authorizationDatabase)
            .WithReference(redis)
            .WithReference(rabbitmq);

        var catalogApplication = builder.AddProject<Projects.EShop_Catalog_Application>(ResourceNames.CatalogWriteApi)
            .WithExternalServiceMode(useExternalService)
            .WithReference(catalogDatabase)
            .WithReference(redis)
            .WithReference(rabbitmq);

        var catalogReadModel = builder.AddProject<Projects.EShop_Catalog_ReadModels_MongoDb>(ResourceNames.CatalogReadApi)
            .WithExternalServiceMode(useExternalService)
            .WithReference(catalogMongoDatabase)
            .WithReference(redis)
            .WithReference(rabbitmq);

        var inventory = builder.AddProject<Projects.EShop_Inventory_API>(ResourceNames.InventoryApi)
            .WithExternalServiceMode(useExternalService)
            .WithReference(inventoryDatabase)
            .WithReference(redis)
            .WithReference(rabbitmq);

        var order = builder.AddProject<Projects.EShop_Order_API>(ResourceNames.OrderApi)
            .WithExternalServiceMode(useExternalService)
            .WithReference(orderDatabase)
            .WithReference(redis)
            .WithReference(rabbitmq);

        var finance = builder.AddProject<Projects.EShop_Finance_API>(ResourceNames.FinanceApi)
            .WithExternalServiceMode(useExternalService)
            .WithReference(financeDatabase)
            .WithReference(redis)
            .WithReference(rabbitmq);

        if (!useExternalService)
        {
            tenancy.WaitFor(tenancyDatabase)
                .WaitFor(redis)
                .WaitFor(rabbitmq);

            authorization.WaitFor(authorizationDatabase)
                .WaitFor(redis)
                .WaitFor(rabbitmq);

            catalogApplication.WaitFor(catalogDatabase)
                .WaitFor(redis)
                .WaitFor(rabbitmq);

            catalogReadModel
                .WaitFor(catalogMongoDatabase)
                .WaitFor(redis)
                .WaitFor(rabbitmq)
                .WaitFor(catalogApplication);

            inventory.WaitFor(inventoryDatabase)
                .WaitFor(redis)
                .WaitFor(rabbitmq);

            order.WaitFor(orderDatabase)
                .WaitFor(redis)
                .WaitFor(rabbitmq);

            finance.WaitFor(financeDatabase)
                .WaitFor(redis)
                .WaitFor(rabbitmq);

            if (grafana is not null)
            {
                var grafanaEndpoint = grafana.GetEndpoint("http");
                tenancy.WithEnvironment("GRAFANA_URL", grafanaEndpoint);
                authorization.WithEnvironment("GRAFANA_URL", grafanaEndpoint);
                catalogApplication.WithEnvironment("GRAFANA_URL", grafanaEndpoint);
                catalogReadModel.WithEnvironment("GRAFANA_URL", grafanaEndpoint);
                inventory.WithEnvironment("GRAFANA_URL", grafanaEndpoint);
                order.WithEnvironment("GRAFANA_URL", grafanaEndpoint);
                finance.WithEnvironment("GRAFANA_URL", grafanaEndpoint);
            }
        }

        #endregion

        #region API Gateway - YARP Reserve Proxy

        //var apiGateway = builder.AddProject<Projects.EShop_ApiGateway>(ResourceNames.ApiGateway)
        //    .WithReference(redis)
        //    .WithReference(tenancy)
        //    .WithReference(authorization)
        //    .WithReference(catalogApplication)
        //    .WithReference(catalogReadModel)
        //    .WithReference(inventory)
        //    .WithReference(order)
        //    .WithReference(finance);

        //if (!useExternalService)
        //{
        //    apiGateway
        //        .WaitFor(redis)
        //        .WaitFor(tenancy)
        //        .WaitFor(authorization)
        //        .WaitFor(catalogApplication)
        //        .WaitFor(catalogReadModel)
        //        .WaitFor(inventory)
        //        .WaitFor(order)
        //        .WaitFor(finance);
        //}

        #endregion
    }

    public static IResourceBuilder<T> WithExternalServiceMode<T>(this IResourceBuilder<T> builder, bool isExternal) where T : IResourceWithEnvironment
    {
        return builder.WithEnvironment("IsExternalServiceMode", isExternal.ToString());
    }
}
