using Aspire.Hosting.Lifecycle;
using Microsoft.Extensions.Logging;

namespace EShop.AppHost.OpenTelemetryCollector;

internal sealed class OpenTelemetryCollectorLifecycleHook : IDistributedApplicationLifecycleHook
{
    private const string OtelExporterOtlpEndpoint = "OTEL_EXPORTER_OTLP_ENDPOINT";

    private readonly ILogger<OpenTelemetryCollectorLifecycleHook> _logger;

    public OpenTelemetryCollectorLifecycleHook(ILogger<OpenTelemetryCollectorLifecycleHook> logger)
    {
        _logger = logger;
    }

    public Task AfterEndpointsAllocatedAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken)
    {
        var collectorResource = appModel.Resources.OfType<OpenTelemetryCollectorResource>().FirstOrDefault();
        if (collectorResource is null)
        {
            _logger.LogWarning($"No {nameof(OpenTelemetryCollectorResource)} resource found.");
            return Task.CompletedTask;
        }

        var endpoint = collectorResource.GetEndpoint(OpenTelemetryCollectorResource.OtlpGrpcEndpointName);
        if (!endpoint.Exists)
        {
            _logger.LogWarning($"No '{OpenTelemetryCollectorResource.OtlpGrpcEndpointName}' endpoint found on {nameof(OpenTelemetryCollectorResource)}.");
            return Task.CompletedTask;
        }

        foreach (var resource in appModel.Resources)
        {
            resource.Annotations.Add(new EnvironmentCallbackAnnotation((context) =>
            {
                if (context.EnvironmentVariables.ContainsKey(OtelExporterOtlpEndpoint))
                {
                    _logger.LogDebug("Forwarding telemetry for {ResourceName} to the collector ({Url}).", resource.Name, endpoint.Url);

                    context.EnvironmentVariables[OtelExporterOtlpEndpoint] = endpoint;
                }
            }));
        }

        return Task.CompletedTask;
    }
}