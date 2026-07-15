namespace EShop.Tenancy.Application.UseCases.Tenants.GetRateLimitPolicy;

public sealed class TenantRateLimitPolicyResponse
{
    public required bool HasPolicy { get; init; }
    public required IReadOnlyList<RateLimitRuleResponse> Rules { get; init; }
}

public sealed class RateLimitRuleResponse
{
    public required string Domain { get; init; }
    public required string Scope { get; init; }
    public required string Unit { get; init; }
    public required int RequestsPerUnit { get; init; }
    public int? Burst { get; init; }
}
