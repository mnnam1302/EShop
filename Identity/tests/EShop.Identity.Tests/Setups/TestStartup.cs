using EShop.Identity.API.DependencyInjections.Extensions;
using EShop.Identity.API.Middlewares;
using EShop.Identity.Application.DependencyInjections.Extensions;
using EShop.Identity.Infrastructure.DependencyInjections.Extensions;
using EShop.Identity.Persistence;
using EShop.Identity.Persistence.DependencyInjections.Extensions;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.Cache.Providers;
using EShop.Shared.Cache.Services;
using EShop.Shared.JsonApi.DependencyInjections;
using EShop.Shared.Scoping.ResourceAccessControl.Providers;
using EShop.Testing.JsonApiApplication;
using EShop.Testing.JsonApiApplication.DependencyInjections;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Research run parallel testin with Xunit and Reqnroll: https://docs.reqnroll.net/latest/execution/parallel-execution.html
namespace EShop.Identity.Tests.Setups;

public class TestStartup : Identity.API.Startup
{
    private readonly PostgreSqlTestDatabase _testDatabase;
    
    public TestStartup(IConfiguration configuration, IWebHostEnvironment env, PostgreSqlTestDatabase testDatabase)
        : base(configuration, env)
    {
        this.Environment.EnvironmentName = "Development";
        _testDatabase = testDatabase;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddCors();

        services.AddTransient(typeof(CachedRemoteConfiguration));
        services.AddTransient<IRedisResiliencePolicyProvider, RedisResiliencePolicyProvider>();
        services.AddDistributedMemoryCache();
        services.AddUserTokenCachingService();
        services.AddUserPermissionForOwnerService();

        services.AddPostgreSqlTestDbContext<UsersDbContext>(_testDatabase);

        // Middleware
        services.AddTransient<ExceptionHandlingMiddleware>();

        /*
         * API
         * - Controllers
         * - Api Versioning
         * - Swagger
         * - Health Checks
         * - Logging - shared
         */
        services.AddControllers()
            .AddApplicationPart(typeof(Identity.API.Startup).Assembly);

        //services.AddMvc(options =>
        //{
        //    options.AddDefaultAuthorizationFilter();
        //})
        //    .AddNewtonsoftJson(options => options.UseCamelCasing(processDictionaryKeys: false))
        //    .AddApplicationPart(typeof(Identity.API.Startup).Assembly)
        //    .AddControllersAsServices();

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

        services.ReplaceAll<IAsyncRedisCachingService, FakeRedisCachingService>(ServiceLifetime.Singleton);
        services.ReplaceAll<IRedisCachingService, FakeRedisCachingService>(ServiceLifetime.Singleton);

        /*
         * Application
         * - Automapper
         * - MediatR
         */
        services.AddMediatRApplication();
        services.AddAutoMapperApplication();

        // Persistence
        services.AddRepositoryAndUnitOfWorkPersistence();

        // Infrastructure
        services.AddServicesInfrastructure();
    }

    public override void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}