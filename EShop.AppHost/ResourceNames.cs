namespace EShop.AppHost;

public static class ResourceNames
{
    // observability
    public const string OpenTelemetryCollector = "otel-collector";

    // infrastructure
    public const string PostgreSql = "postgres";
    public const string Redis = "redis";
    public const string RabbitMq = "rabbitmq";
    public const string MongoDb = "mongodb";

    // microservices
    public const string TenancyApi = "tenancy";
    public const string AuthorizationApi = "authorization";
}