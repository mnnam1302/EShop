namespace EShop.Finance.Infrastructure.Integration;

public class IntegrationProviderSession
{
    public required string TenantId { get; set; }

    public string? SessionToken { get; set; }

    public DateTimeOffset? ExpiresAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
