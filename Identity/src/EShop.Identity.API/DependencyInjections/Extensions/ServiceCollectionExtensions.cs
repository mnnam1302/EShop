using EShop.Identity.API.Middlewares;
using EShop.Identity.Application.DependencyInjections.Extensions;
using EShop.Identity.Application.Services;
using EShop.Identity.Infrastructure.DependencyInjections.Extensions;
using EShop.Identity.Persistence;
using EShop.Identity.Persistence.DependencyInjections.Extensions;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.JsonApi.DependencyInjections;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;

namespace EShop.Identity.API.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShared(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services
            .AddRedisInfrastructure(configuration)
            .AddUserTokenCachingService();

        services
            .AddDbContextWithScoping<UsersDbContext>(configuration);

        return services;
    }

    public static IServiceCollection AddBoostrapping(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services
            .AddCors()
            .AddServicesApiLayer()
            .AddServicesApplicationLayer()
            .AddServicesPersistenceLayer()
            .AddServicesInfrastructureLayer()
            .AddUserPermissionForOwnerService();

        return services;
    }

    public static IServiceCollection AddServicesApiLayer(this IServiceCollection services)
    {
        services.AddSingleton<ExceptionHandlingMiddleware>();

        services
            .AddControllers()
            .AddApplicationPart(Identity.Presentation.AssemblyReference.Assembly);

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

        return services;
    }

    public static IServiceCollection AddUserPermissionForOwnerService(
        this IServiceCollection services)
    {
        services.AddTransient<IPermissionValidator, CurrentUserPermissionsValidator>();
        AddPermissionCachingServiceForOwnService(services);
        return services;
    }

    private static void AddPermissionCachingServiceForOwnService(IServiceCollection services)
    {
        services.AddTransient<IRedisCachingProvider<string[]>, RedisCachingProvider<string[]>>();
        services.AddTransient<IPermissionCachingOwnerService, PermissionRedisCachingService>();
        services.AddTransient<IPermissionCalculator, PermissionCalculator>();
        services.AddTransient<IUserPermissionsProvider, OwnerCacheUserPermissionService>();
    }
}