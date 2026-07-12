using EShop.Shared.Contracts.Abstractions.Mediator;
using EShop.Tenancy.Domain.Enumerations;

namespace EShop.Tenancy.Application.UseCases.Tenants.SetRateLimitPolicy;

public sealed class SetTenantRateLimitPolicyCommand : ICommand
{
    public required string TenantId { get; init; }
    public required IReadOnlyList<RateLimitRuleInput> Rules { get; init; }
}

public sealed class RateLimitRuleInput
{
    public required string Domain { get; init; }
    public required RateLimitScope Scope { get; init; }
    public required RateLimitUnit Unit { get; init; }
    public required int RequestsPerUnit { get; init; }
    public int? Burst { get; init; }
}
