using EShop.Tenancy.Domain.Enumerations;

namespace EShop.Tenancy.Application.UseCases.Tenants.GetRateLimitPolicy;

public sealed class TenantRateLimitPolicyResponse
{
    public required bool HasPolicy { get; init; }
    public required IReadOnlyList<RateLimitRuleResponse> Rules { get; init; }
}

public sealed class RateLimitRuleResponse
{
    public required string Domain { get; init; }
    public required RateLimitScope Scope { get; init; }
    public required RateLimitUnit Unit { get; init; }
    public required int RequestsPerUnit { get; init; }
    public int? Burst { get; init; }
}
