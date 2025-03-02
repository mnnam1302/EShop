using EShop.Identity.API.DependencyInjections.Extensions;
using EShop.Identity.API.Middlewares;
using EShop.Identity.Application.DependencyInjections.Extensions;
using EShop.Identity.Infrastructure.DependencyInjections.Extensions;
using EShop.Identity.Persistence;
using EShop.Identity.Persistence.DependencyInjections.Extensions;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.Contracts.Services.Identity.Auth;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserPermissionProvider;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserTokenProvider;
using EShop.Testing.JsonApiApplication;
using EShop.Testing.JsonApiApplication.DependencyInjections;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EShop.Identity.Tests.Setups;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTestBoostrapping(this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services
            .AddCors()
            .AddTestServicesApiLayer()
            .AddIdentityApplication()
            .AddIdentityPersistence()
            .AddIdentityInfrastructure()
            .AddTestUserPermissions(); // because user permissions related cache and user services, thus it's putted

        return services;
    }

    private static IServiceCollection AddTestUserPermissions(this IServiceCollection services)
    {
        services.AddTransient<IPermissionValidator, CurrentUserPermissionsValidator>();
        services.AddSingleton<IUserPermissionsProvider, TestUserPermissionProvider>();

        return services;
    }

    private static IServiceCollection AddTestServicesApiLayer(this IServiceCollection services)
    {
        services.AddTransient<ExceptionHandlingMiddleware>();

        services.AddControllers()
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

    public static IServiceCollection AddTestShared(this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        PostgreSqlTestDatabase testDatabase)
    {
        services
            .AddMemoryInfrastructure()
            .AddUserTokenCachingService();

        services
            .AddPostgreSqlTestDbContext<UsersDbContext>(testDatabase);

        return services;
    }

    // Consider
    private static IServiceCollection AddTestUserTokens(this IServiceCollection services)
    {
        services.AddTransient<IRedisCachingProvider<Response.AuthenticatedResponse>, TestUserTokenProvider>();
        services.AddTransient<ITokenCachingService, TokenRedisCachingService>();

        return services;
    }
}