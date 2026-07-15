using EShop.Shared.Contracts.Abstractions.Mediator;

namespace EShop.Tenancy.Application.UseCases.Tenants.GetRateLimitPolicy;

public sealed record GetTenantRateLimitPolicyQuery(string TenantId) : IQuery<TenantRateLimitPolicyResponse>;
