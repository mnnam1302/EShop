namespace EShop.Shared.Contracts.Services.Tenancy.Features;

public static class Response
{
    public record TenantFeaturesResponse
    {
        public string[] FeatureIds { get; set; } = Array.Empty<string>();
    }
}