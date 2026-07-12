namespace EShop.Tenancy.Presentation.Models;

public sealed class SetRateLimitPolicyRequest
{
    public required IReadOnlyList<RateLimitRuleRequest> Rules { get; init; }
}

public sealed class RateLimitRuleRequest
{
    public required string Domain { get; init; }
    public required string Scope { get; init; }
    public required string Unit { get; init; }
    public required int RequestsPerUnit { get; init; }
    public int? Burst { get; init; }
}
