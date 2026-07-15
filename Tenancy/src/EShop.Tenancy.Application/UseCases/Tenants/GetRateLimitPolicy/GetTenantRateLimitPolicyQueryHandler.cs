using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Query;
using EShop.Tenancy.Domain.Abstractions.Repositories;

namespace EShop.Tenancy.Application.UseCases.Tenants.GetRateLimitPolicy;

internal sealed class GetTenantRateLimitPolicyQueryHandler(
    IUserDetailsProvider userDetailsProvider,
    ITenantRepository tenantRepository) : IQueryHandler<GetTenantRateLimitPolicyQuery, TenantRateLimitPolicyResponse>
{
    public async Task<Result<TenantRateLimitPolicyResponse>> HandleAsync(GetTenantRateLimitPolicyQuery query, CancellationToken cancellationToken = default)
    {
        using var scope = userDetailsProvider.CreateSystemUserScope(query.TenantId);

        var tenant = await tenantRepository.FindByIdAsync(
            query.TenantId,
            includeProperties: t => t.TenantSettings,
            cancellationToken: cancellationToken);

        if (tenant is null)
        {
            return Result.Failure<TenantRateLimitPolicyResponse>(new("Tenant.NotFound", $"Tenant with ID '{query.TenantId}' not found."));
        }

        var policy = tenant.TenantSettings.SingleOrDefault()?.RateLimitPolicy;

        var response = new TenantRateLimitPolicyResponse
        {
            HasPolicy = policy is not null,
            Rules = policy?.Rules.Select(rule => new RateLimitRuleResponse
            {
                Domain = rule.Domain,
                Scope = rule.Scope,
                Unit = rule.Unit,
                RequestsPerUnit = rule.RequestsPerUnit,
                Burst = rule.Burst
            }).ToList() ?? []
        };

        return Result.Success(response);
    }
}
