using EShop.Identity.API.DependencyInjections.Extensions;
using EShop.Identity.API.Middlewares;
using EShop.Identity.Application.DependencyInjections.Extensions;
using EShop.Identity.Infrastructure.DependencyInjections.Extensions;
using EShop.Identity.Persistence;
using EShop.Identity.Persistence.DependencyInjections.Extensions;
using EShop.Shared.Cache.DependencyInejctions.Extensions;
using EShop.Shared.JsonApi.DependencyInjections;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;

namespace EShop.Identity.API;

public class Startup
{
    public IConfiguration Configuration { get; }
    public IWebHostEnvironment Environment { get; }

    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        Configuration = configuration;
        Environment = environment;
    }

    public virtual void ConfigureServices(IServiceCollection services)
    {
        // Shared - Common
        services.AddCors();

        services.AddRedisInfrastructure(Configuration);
        services.AddUserTokenCachingService();

        services.AddDbContextWithScoping<UserDbContext>(Configuration, false);
        services.AddTransient<DbInitializer>();

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
        services.AddControllers();
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

        services.AddUserPermissionForOwnerServiceAPI();

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

    public virtual void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (Environment.IsDevelopment() || Environment.IsStaging())
        {
            app.UseCors(x => x.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod());
            app.UseSwaggerAPI();
        }

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}