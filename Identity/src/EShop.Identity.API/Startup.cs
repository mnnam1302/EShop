using EShop.Identity.API.DependencyInjections.Extensions;
using EShop.Identity.API.Middlewares;

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
        /*
         * Refactor clean code
         * - AddBoostrapping
         * - AddShared
         */

        services
            .AddShared(Configuration, Environment)
            .AddBoostrapping(Configuration, Environment);

        // Shared - Common
        //services.AddCors();

        //services.AddRedisInfrastructure(Configuration);
        //services.AddUserTokenCachingService();

        //services.AddDbContextWithScoping<UsersDbContext>(Configuration, false);
        //services.AddTransient<DbInitializer>();

        // Middleware
        //services.AddTransient<ExceptionHandlingMiddleware>();

        /*
         * API
         * - Controllers
         * - Api Versioning
         * - Swagger
         * - Health Checks
         * - Logging - shared
         */
        //services.AddControllers();
        //services
        //    .AddSwaggerGenNewtonsoftSupport()
        //    .AddFluentValidationRulesToSwagger()
        //    .AddEndpointsApiExplorer()
        //    .AddSwaggerAPI();

        //services
        //    .AddApiVersioning(options => options.ReportApiVersions = true)
        //    .AddApiExplorer(options =>
        //    {
        //        options.GroupNameFormat = "'v'VVV";
        //        options.SubstituteApiVersionInUrl = true;
        //    });

        //services.AddUserPermissionForOwnerService();

        /*
         * Application
         * - Automapper
         * - MediatR
         */
        //services.AddMediatRApplication();
        //services.AddAutoMapperApplication();

        // Persistence
        //services.AddRepositoryAndUnitOfWorkPersistence();

        // Infrastructure
        //services.AddServicesInfrastructure();
    }

    public virtual void Configure(IApplicationBuilder app, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (Environment.IsDevelopment() || Environment.IsStaging())
        {
            app.UseDeveloperExceptionPage();
            app.UseCors(x => x.AllowAnyMethod());
            app.UseSwaggerAPI();
        }

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}