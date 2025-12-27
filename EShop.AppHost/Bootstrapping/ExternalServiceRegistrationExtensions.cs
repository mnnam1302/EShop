using Aspire.Hosting;

namespace EShop.AppHost.Bootstrapping;

public static class ExternalServiceRegistrationExtensions
{
    private const string ConnectionStringEnvironmentName = "ConnectionStrings";

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
        var redis = useExternalService
            ? builder.AddConnectionString(ServiceConnectionNames.Redis)
            : builder
                .AddRedis(ServiceConnectionNames.Redis)
                .WithImageTag("latest")
                .WithLifetime(ContainerLifetime.Persistent)
                .WithRedisInsight();

        var postgresql = useExternalService
            ? builder.AddConnectionString(ServiceConnectionNames.PostgreSql)
            : builder
                .AddPostgres(ServiceConnectionNames.PostgreSql)
                .WithLifetime(ContainerLifetime.Persistent)
                .WithPgAdmin()
                .WithDataVolume();

        var rabbitmq = useExternalService
            ? builder.AddConnectionString(ServiceConnectionNames.RabbitMq)
            : builder
                .AddRabbitMQ(ServiceConnectionNames.RabbitMq)
                .WithLifetime(ContainerLifetime.Persistent)
                .WithManagementPlugin();

        builder.AddProject<Projects.EShop_Tenancy_API>(ServiceConnectionNames.TenancyApi)
            .WithExternalServiceMode(useExternalService)
            .WithReference(postgresql)
            .WithReference(redis)
            .WithReference(rabbitmq);

        return builder;
    }

    public static IResourceBuilder<T> WithExternalServiceMode<T>(this IResourceBuilder<T> builder, bool isExternal) where T : IResourceWithEnvironment
    {
        return builder.WithEnvironment("IsExternalServiceMode", isExternal.ToString());
    }
}