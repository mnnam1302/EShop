using EShop.Shared.JsonApi.DependencyInjections;
using EShop.Shared.JsonApi.Middlewares;

namespace EShop.Configuration.Application.Shared;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ExceptionHandlingMiddleware>();

        services
            .AddPostgreSqlHealthCheck(configuration)
            .AddDbContextPoolWithScoping<ConfigurationDbContext>(configuration);

        return services;
    }
}