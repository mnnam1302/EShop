using EShop.AppHost.OpenTelemetryCollector;

namespace EShop.AppHost.Extensions;

public static class ExternalServiceRegistrationExtensions
{
    public static IDistributedApplicationBuilder AddExternalServices(this IDistributedApplicationBuilder builder, bool useExternalObservability)
    {
        return AddServices(
            builder,
            useExternalInfrastructure: true,
            useExternalObservability);
    }

    public static IDistributedApplicationBuilder AddServiceDefaults(this IDistributedApplicationBuilder builder, bool useExternalObservability)
    {
        return AddServices(
            builder,
            useExternalInfrastructure: false,
            useExternalObservability);
    }

    private static IDistributedApplicationBuilder AddServices(IDistributedApplicationBuilder builder, bool useExternalInfrastructure, bool useExternalObservability)
    {
        #region Observability

        Action<IResourceBuilder<IResourceWithEnvironment>>? applyGrafanaUrl;

        if (useExternalObservability)
        {
            var externalGrafana = builder.AddConnectionString(ResourceNames.Grafana);
            applyGrafanaUrl = resource => resource.WithEnvironment("GRAFANA_URL", externalGrafana);
        }
        else
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

            // Pull approach: OTelCollector + Prometheus
            // Auto add environment variable "OTEL_EXPORTER_OTLP_ENDPOINT" into microservice resources to enable OTel configuration
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

            var grafana = builder.AddContainer(ResourceNames.Grafana, "grafana/grafana")
                .WithBindMount("../deploy/config/grafana/provisioning", "/etc/grafana/provisioning", isReadOnly: true)
                .WithBindMount("../deploy/config/grafana/dashboards", "/var/lib/grafana/dashboards", isReadOnly: true)
                .WithEnvironment("PROMETHEUS_ENDPOINT", prometheus.GetEndpoint("http"))
                .WithHttpEndpoint(targetPort: 3000, name: "http")
                .WaitFor(prometheus);

            var grafanaEndpoint = grafana.GetEndpoint("http");
            applyGrafanaUrl = resource => resource.WithEnvironment("GRAFANA_URL", grafanaEndpoint);
        }

        #endregion Observability

        #region Infrastructure resources

        var redis = useExternalInfrastructure
            ? builder.AddConnectionString(ResourceNames.Redis)
            : builder
                .AddRedis(ResourceNames.Redis)
                .WithImageTag("7.4.7")
                .WithDataVolume("eshop-redis")
                .WithRedisInsight()
                .WithLifetime(ContainerLifetime.Persistent);

        var rabbitmq = useExternalInfrastructure
            ? builder.AddConnectionString(ResourceNames.RabbitMq)
            : builder
                .AddRabbitMQ(ResourceNames.RabbitMq)
                .WithImageTag("3")
                .WithDataVolume("eshop-rabbitmq")
                .WithLifetime(ContainerLifetime.Persistent)
                .WithManagementPlugin();

        #endregion Infrastructure resources

        if (useExternalInfrastructure)
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
                useExternalInfrastructure,
                redis,
                rabbitmq,
                tenancyDatabase,
                authorizationDatabase,
                catalogWriteDatabase,
                catalogReadDatabase,
                inventoryDatabase,
                orderDatabase,
                financeDatabase,
                applyGrafanaUrl);
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
                useExternalInfrastructure,
                redis,
                rabbitmq,
                tenancyDatabase,
                authorizationDatabase,
                catalogWriteDatabase,
                catalogReadDatabase,
                inventoryDatabase,
                orderDatabase,
                financeDatabase,
                applyGrafanaUrl);
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
        Action<IResourceBuilder<IResourceWithEnvironment>>? applyGrafanaUrl)
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
        }

        //if (applyGrafanaUrl is not null)
        //{
        //    applyGrafanaUrl(tenancy);
        //    applyGrafanaUrl(authorization);
        //    applyGrafanaUrl(catalogApplication);
        //    applyGrafanaUrl(catalogReadModel);
        //    applyGrafanaUrl(inventory);
        //    applyGrafanaUrl(order);
        //    applyGrafanaUrl(finance);
        //}

        #endregion

        #region API Gateway - YARP Reserve Proxy

        // .WithReplicas(2) for task 9.1 (distributed-rate-limiter): verifying the cross-replica
        // scenario requires two gateway processes actually running side by side against the same
        // Redis, not just two RedisRateLimiter instances in a test.
        var apiGateway = builder.AddProject<Projects.EShop_ApiGateway>(ResourceNames.ApiGateway)
            //.WithReplicas(2) // uncomment when dev test - rate limting share multiple-servers
            .WithReference(redis)
            .WithReference(tenancy)
            .WithReference(authorization)
            .WithReference(catalogApplication)
            .WithReference(catalogReadModel)
            .WithReference(inventory)
            .WithReference(order)
            .WithReference(finance);

        if (!useExternalService)
        {
            apiGateway
                .WaitFor(redis)
                .WaitFor(tenancy)
                .WaitFor(authorization)
                .WaitFor(catalogApplication)
                .WaitFor(catalogReadModel)
                .WaitFor(inventory)
                .WaitFor(order)
                .WaitFor(finance);
        }

        #endregion
    }

    public static IResourceBuilder<T> WithExternalServiceMode<T>(this IResourceBuilder<T> builder, bool isExternal) where T : IResourceWithEnvironment
    {
        return builder.WithEnvironment("IsExternalServiceMode", isExternal.ToString());
    }
}
