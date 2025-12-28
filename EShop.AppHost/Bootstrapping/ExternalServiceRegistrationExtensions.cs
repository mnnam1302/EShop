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
        // Infrastructure resources
        var pathToDbInitDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\Deployment\Scripts\"));
        var postgres = builder.AddPostgres(ServiceConnectionNames.PostgreSql)
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
            ? builder.AddConnectionString(ServiceConnectionNames.Redis)
            : builder.AddRedis(ServiceConnectionNames.Redis)
                .WithRedisInsight(rb =>
                {
                    rb.WithLifetime(ContainerLifetime.Persistent);
                    rb.WithHostPort(6389);
                })
                .WithLifetime(ContainerLifetime.Persistent);

        var rabbitmq = useExternalService
            ? builder.AddConnectionString(ServiceConnectionNames.RabbitMq)
            : builder
                .AddRabbitMQ(ServiceConnectionNames.RabbitMq)
                .WithLifetime(ContainerLifetime.Persistent)
                .WithManagementPlugin(port: 15672);

        // Tenancy Microservice
        var tenancyDatabase = postgres.AddDatabase("tenancyDatabase", "eshop_tenancy");
        var tenancy = builder.AddProject<Projects.EShop_Tenancy_API>(ServiceConnectionNames.TenancyApi)
            .WithExternalServiceMode(useExternalService)
            .WithReference(postgres)
            .WithReference(redis)
            .WithReference(rabbitmq);

        if (!useExternalService)
        {
            tenancy
                .WaitFor(tenancyDatabase)
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