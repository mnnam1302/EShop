using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace EShop.Finance.Application.Services.IntegrationProvider.Configuration;

public static class ProviderConfigurationParser
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public static FinanceConfiguration Parse(string? yaml)
    {
        if (string.IsNullOrWhiteSpace(yaml))
        {
            return new FinanceConfiguration();
        }

        try
        {
            var config = Deserializer.Deserialize<FinanceConfiguration>(yaml) ?? new FinanceConfiguration();
            config.InitializeLookupCache();
            return config;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse YAML provider configuration: {ex.Message}", ex);
        }
    }
}
