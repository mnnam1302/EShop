using Microsoft.Extensions.Configuration;
using Serilog;
using System.Reflection;

namespace EShop.Shared.Diagnostics;

public static class Logging
{
    public static void SetSerilog(string appShortName)
    {
        var serilogConfiguration = new ConfigurationBuilder()
            .SetBasePath(System.IO.Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        Serilog.Debugging.SelfLog.Enable(Console.Out);
        var appFullName = Assembly.GetEntryAssembly()?.GetName();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(serilogConfiguration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("ApplicationName", appShortName ?? appFullName?.Name)
            .Enrich.WithProperty("ApplicationVersion", appFullName?.Version)
            .CreateLogger();
    }
}