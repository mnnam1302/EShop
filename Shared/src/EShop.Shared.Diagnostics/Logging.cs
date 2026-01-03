using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
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

        var loggerConfiguration = new LoggerConfiguration()
            .ReadFrom.Configuration(serilogConfiguration)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("ApplicationName", appShortName ?? appFullName?.Name)
            .Enrich.WithProperty("ApplicationVersion", appFullName?.Version);

        // Configure OpenTelemetry sink if OTLP endpoint is configured
        //var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
        //if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        //{
        //    var otlpProtocol = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_PROTOCOL") ?? "grpc";
        //    var otlpHeaders = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_HEADERS");
        //    var serviceName = Environment.GetEnvironmentVariable("OTEL_SERVICE_NAME") ?? appShortName;

        //    loggerConfiguration.WriteTo.OpenTelemetry(options =>
        //    {
        //        options.Endpoint = otlpProtocol.Equals("grpc", StringComparison.OrdinalIgnoreCase)
        //            ? otlpEndpoint
        //            : $"{otlpEndpoint.TrimEnd('/')}/v1/logs";

        //        options.Protocol = otlpProtocol.Equals("grpc", StringComparison.OrdinalIgnoreCase)
        //            ? OtlpProtocol.Grpc
        //            : OtlpProtocol.HttpProtobuf;

        //        options.ResourceAttributes = new Dictionary<string, object>
        //        {
        //            ["service.name"] = serviceName ?? "unknown-service"
        //        };

        //        // Parse and add OTLP headers (format: key=value,key2=value2)
        //        if (!string.IsNullOrWhiteSpace(otlpHeaders))
        //        {
        //            foreach (var header in otlpHeaders.Split(',', StringSplitOptions.RemoveEmptyEntries))
        //            {
        //                var parts = header.Split('=', 2);
        //                if (parts.Length == 2)
        //                {
        //                    options.Headers[parts[0].Trim()] = parts[1].Trim();
        //                }
        //            }
        //        }
        //    });
        //}

        Log.Logger = loggerConfiguration.CreateLogger();
    }
}