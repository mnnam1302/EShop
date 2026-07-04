namespace Microsoft.Extensions.Hosting;

public sealed class OtlpExporterSettings
{
    public const string SectionName = "Otlp";

    public string Endpoint { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
