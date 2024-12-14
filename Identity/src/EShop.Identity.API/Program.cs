using EShop.Identity.Persistence;
using EShop.Shared.Diagnostics;
using Serilog;

namespace EShop.Identity.API;

public static class Program
{
    private const int ShutdownTimeoutInSeconds = 90;
    internal const string ApplicationName = "Identity";

    public static async Task<int> Main(string[] args)
    {
        Logging.SetSerilog(ApplicationName);

        Log.Information("Initilizing {ApplicationName} ....", ApplicationName);

        try
        {
            var host = CreateHostBuilder(args).Build();
            //var app = CreateBuilder(args).Build();

            await using (var scope = host.Services.CreateAsyncScope())
            {
                var services = scope.ServiceProvider;
                var dbInitializer = services.GetRequiredService<DbInitializer>();
                await dbInitializer.Initialize();
            }

            Log.Information("Starting up {ApplicationName}...", ApplicationName);
            await host.RunAsync();
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

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        // generic host: https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host?tabs=appbuilder
        return Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>()
                    .UseShutdownTimeout(TimeSpan.FromSeconds(ShutdownTimeoutInSeconds));
            })
            .UseSerilog();
    }

    private static WebApplicationBuilder CreateBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>()
                    .UseShutdownTimeout(TimeSpan.FromSeconds(ShutdownTimeoutInSeconds));
            })
            .UseSerilog();

        return builder;
    }
}