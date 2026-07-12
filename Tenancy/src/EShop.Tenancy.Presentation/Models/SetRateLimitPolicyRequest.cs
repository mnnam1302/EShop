using EShop.Tenancy.Domain.Enumerations;

namespace EShop.Tenancy.Presentation.Models;

public sealed class SetRateLimitPolicyRequest
{
    public required IReadOnlyList<RateLimitRuleRequest> Rules { get; init; }
}

public sealed class RateLimitRuleRequest
{
    public required string Domain { get; init; }
    public required RateLimitScope Scope { get; init; }
    public required RateLimitUnit Unit { get; init; }
    public required int RequestsPerUnit { get; init; }
    public int? Burst { get; init; }
}
