using EShop.Catalog.Application.Bootstrapping;
using EShop.Shared.Diagnostics;
using Serilog;

namespace EShop.Catalog.Application;

public class Program
{
    private const int ShutdownTimeoutInSeconds = 90;
    internal const string ApplicationName = "Catalog";

    public static async Task<int> Main(string[] args)
    {
        Logging.SetSerilog(ApplicationName);

        Log.Information("Initilizing {ApplicationName} ....", ApplicationName);

        try
        {
            var app = BuidlWebApp(args);

            await using (var scope = app.Services.CreateAsyncScope())
            {
                var dbInitializer = ActivatorUtilities.CreateInstance<DbInitializer>(scope.ServiceProvider);
                await dbInitializer.Initialize();
            }

            Log.Information("Starting up {ApplicationName}...", ApplicationName);
            await app.RunAsync();
            Log.Information("Stop {ApplicationName}...", ApplicationName);
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static WebApplication BuidlWebApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        var startup = new Startup(builder.Configuration, builder.Environment);
        startup.ConfigureServices(builder.Services);

        builder.Host.UseSerilog();
        builder.WebHost.UseShutdownTimeout(TimeSpan.FromSeconds(ShutdownTimeoutInSeconds));

        var app = builder.Build();

        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        startup.Configure(app, app.Lifetime, loggerFactory);

        return app;
    }
}
