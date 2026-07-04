namespace EShop.AppHost.OpenTelemetryCollector;

internal sealed class OpenTelemetryCollectorResource : ContainerResource
{
    internal const string OtlpGrpcEndpointName = "grpc";
    internal const string OtlpHttpEndpointName = "http";

    public OpenTelemetryCollectorResource(string name) : base(name)
    {
    }
}
