namespace EShop.AppHost;

public static class ResourceNames
{
    // observability
    public const string OpenTelemetryCollector = "otel-collector";
    public const string Prometheus = "prometheus";
    public const string Grafana = "grafana";

    // infrastructure
    public const string PostgreSql = "postgres";
    public const string Redis = "redis";
    public const string RabbitMq = "rabbitmq";
    public const string MongoDb = "mongodb";

    // microservices
    public const string ApiGateway = "api-gateway";
    public const string TenancyApi = "tenancy";
    public const string AuthorizationApi = "authorization";

    public const string CatalogWriteApi = "catalog-write";
    public const string CatalogReadApi = "catalog-read";
    public const string CatalogSearchApi = "catalog-search";

    public const string InventoryApi = "inventory";
    public const string OrderApi = "order";
}
