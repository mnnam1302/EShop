using EShop.Inventory.API.DependencyInjection;
using EShop.Inventory.Application.DependencyInjection;
using EShop.Inventory.Infrastructure.DependencyInjection;
using EShop.Shared.JsonApi.Extensions;
using EShop.Shared.JsonApi.Middlewares;

namespace EShop.Inventory.API;

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
        services
            .AddInventoryAPI()
            .AddInventoryApplication()
            .AddInventoryPersistence(Configuration, Environment)
            .AddInventoryInfrastructure(Configuration);
    }

    public void Configure(WebApplication app, IHostApplicationLifetime applicationLifetime, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<Startup>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (Environment.IsDevelopment() || Environment.IsStaging())
        {
            app.UseCors(x => x.AllowAnyMethod());
            app.UseSwaggerAPI();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapEndpoints();

        //app.UseHangfireDashboard("/hangfire");

        //RecurringJob.AddOrUpdate<ExpireReservationsJob>(
        //    "expire-reservations",
        //    job => job.ExecuteAsync(),
        //    Cron.Minutely);

        //RecurringJob.AddOrUpdate<SyncRedisStockJob>(
        //    "sync-redis-stock",
        //    job => job.ExecuteAsync(),
        //    "*/5 * * * *");

        app.RegisterFeatures(applicationLifetime, logger);
        app.RegisterPermissions(applicationLifetime, logger);
    }
}
