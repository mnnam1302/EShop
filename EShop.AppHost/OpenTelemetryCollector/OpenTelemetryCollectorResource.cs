namespace EShop.AppHost.OpenTelemetryCollector;

internal class OpenTelemetryCollectorResource(string name) : ContainerResource(name)
{
    internal const string OtlpGrpcEndpointName = "grpc";
    internal const string OtlpHttpEndpointName = "http";
}