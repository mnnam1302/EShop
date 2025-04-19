using EShop.Identity.Application.DependencyInjections.Extensions;
using EShop.Identity.Application.Services;
using EShop.Identity.Infrastructure.DependencyInjections.Extensions;
using EShop.Identity.Persistence;
using EShop.Identity.Persistence.DependencyInjections.Extensions;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.DomainTools.DependencyInjections;
using EShop.Shared.JsonApi.DependencyInjections;
using EShop.Shared.JsonApi.Middlewares;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using static EShop.Shared.Contracts.Services.Identity.Organizations.Response;
using static EShop.Shared.Contracts.Services.Identity.Users.Response;

namespace EShop.Identity.API.DependencyInjections.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddUserScoping();
        services.AddResiliencePolicy();

        // DbContext
        services
            .AddPostgreSqlHealthCheck(configuration)
            .AddDbContextWithScoping<UsersDbContext>(configuration);

        // Redis Cache
        services
            .AddRedisHealthCheck(configuration)
            .AddRedisInfrastructure(configuration)
            .AddUserTokensProvider(configuration)
            .AddTenantFeaturesProvider(configuration);

        return services;
    }

    public static IServiceCollection AddBootstrapping(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Clean architecture
        services
            .AddIdentityApi()
            .AddIdentityApplication()
            .AddIdentityPersistence()
            .AddIdentityInfrastructure(configuration, environment, Program.ApplicationName);

        // Owner service
        services.AddUserPermissionForOwnerService();
        services.AddUserOrganizationContextForOwnerService();

        return services;
    }

    public static IServiceCollection AddIdentityApi(this IServiceCollection services)
    {
        services.AddCors();
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

        services.AddTransient<DbInitializer>();

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
        services.AddTransient<IRedisCachingAsyncProvider<string[]>, RedisCachingAsyncProvider<string[]>>();
        services.AddTransient<IPermissionCachingService, PermissionRedisCachingService>();
        services.AddTransient<IPermissionCalculator, PermissionCalculator>();
        services.AddTransient<IUserPermissionsProvider, OwnerCacheUserPermissionService>();
    }

    public static IServiceCollection AddUserOrganizationContextForOwnerService(this IServiceCollection services)
    {
        services.AddScoped<IUserOrganizationContextCachingService, UserOrganizationContextCachingService>();
        services.AddScoped<IRedisCachingAsyncProvider<UserOrganizationContext>, RedisCachingAsyncProvider<UserOrganizationContext>>();

        services.AddScoped<IOrganizationContextCachingService, OrganizationContextCachingService>();
        services.AddScoped<IRedisCachingAsyncProvider<OrganizationContext>, RedisCachingAsyncProvider<OrganizationContext>>();
        
        services.AddScoped<IUserOrganizationContextCalculator, UserOrganizationContextCalculator>();
        services.AddScoped<IUserOrganizationContextProvider, OwnerCacheUserOrganizationContextService>();

        return services;
    }
}