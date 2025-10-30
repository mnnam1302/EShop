using Carter;
using EShop.Shared.Authentication.DependencyInjections;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.CQRS;
using EShop.Shared.DomainTools.DependencyInjections;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.JsonApi.Middlewares;
using EShop.Tenancy.Application.DependencyInjections;
using EShop.Tenancy.Infrastructure.DependencyInjections;
using EShop.Tenancy.Persistence;
using EShop.Tenancy.Persistence.DependencyInjections.Extensions;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;

namespace EShop.Tenancy.API.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddResiliencePolicy();

        services.AddMediator(Application.AssemblyReference.Assembly);

        services.AddPostgreSqlHealthCheck(configuration)
            .AddDbContextWithScoping<TenancyDbContext>(configuration);

        services.AddRedisHealthCheck(configuration)
            .AddRedisInfrastructure(configuration);

        services.AddUserPermissionsProvider();
        services.AddUserOrganizationContextProvider();

        return services;
    }

    public static IServiceCollection AddBoostrapping(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddTenancyAPI();
        services.AddTenancyApplication();
        services.AddTenancyPersistence();
        services.AddTenancyInfrastructure(configuration, environment, Program.ApplicationName);

        return services;
    }

    public static IServiceCollection AddTenancyAPI(this IServiceCollection services)
    {
        services.AddCors();
        services.AddSingleton<ExceptionHandlingMiddleware>();
        services.AddTransient<DbInitializer>();

        services.AddCarter();

        services
            .AddSwaggerGenNewtonsoftSupport()
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

    public static void AddRsaKeyServices(this IServiceCollection services)
    {
        services.AddMultiTenantKeyManager();
        services.AddRsaKeyCachingProvider();
    }

    public static void AddAuthentication(this IServiceCollection services)
    {
        services.AddJwtTokenAuthentication();
        services.AddUserTokensProvider();
    }
}