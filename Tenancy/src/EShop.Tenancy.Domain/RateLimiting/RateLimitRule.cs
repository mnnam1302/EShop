using EShop.Tenancy.Domain.Enumerations;

namespace EShop.Tenancy.Domain.RateLimiting;

public class RateLimitRule
{
    public const string AllDomains = "*";

    public required string Domain { get; init; }

    public required RateLimitScope Scope { get; init; }

    public required RateLimitUnit Unit { get; init; }

    public required int RequestsPerUnit { get; init; }

    public int? Burst { get; init; }
}
