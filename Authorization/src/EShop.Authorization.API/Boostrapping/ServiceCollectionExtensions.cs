using EShop.Authorization.Application.DependencyInjections;
using EShop.Authorization.Infrastructure;
using EShop.Authorization.Infrastructure.DependencyInjections;
using EShop.Shared.Authentication.DependencyInjections;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.CQRS;
using EShop.Shared.DomainTools.DependencyInjections;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.JsonApi.Middlewares;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;

namespace EShop.Authorization.API.Boostrapping;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddResiliencePolicy();
        services.AddMediator(Application.AssemblyReference.Assembly);

        services.AddPostgreSqlHealthCheck(configuration)
            .AddDbContextWithScoping<AuthorizationDbContext>(configuration);

        services.AddRedisHealthCheck(configuration)
            .AddRedisInfrastructure(configuration);

        return services;
    }

    public static IServiceCollection AddBoostrapping(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services
            .AddAuthorizationAPI()
            .AddAuthorizationApplication()
            .AddAuthorizationPersistence()
            .AddAuthorizationInfrastructure(configuration, environment);

        return services;
    }

    public static IServiceCollection AddAuthorizationAPI(this IServiceCollection services)
    {
        services.AddCors();
        services.AddSingleton<ExceptionHandlingMiddleware>();

        services.AddSwaggerGenNewtonsoftSupport()
            .AddFluentValidationRulesToSwagger()
            .AddEndpointsApiExplorer()
            .AddSwaggerAPI();

        services
            .AddApiVersioning(options => options.ReportApiVersions = true)
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        services.AddRsaKeyServices();
        services.AddAuthentication();

        return services;
    }

    public static IServiceCollection AddAuthentication(this IServiceCollection services)
    {
        services
            .AddJwtTokenAuthentication()
            .AddUserTokensProvider();

        return services;
    }

    public static IServiceCollection AddRsaKeyServices(this IServiceCollection services)
    {
        services
            .AddMultiTenantKeyManager()
            .AddRsaKeyCachingProvider();

        return services;
    }
}
