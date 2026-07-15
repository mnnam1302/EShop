using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.Exceptions;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Tenancy.Domain.Abstractions.Repositories;
using EShop.Tenancy.Domain.Entities;

namespace EShop.Tenancy.Application.UseCases.Tenants.SetRateLimitPolicy;

internal sealed class SetTenantRateLimitPolicyCommandHandler(
    IUserDetailsProvider userDetailsProvider,
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<SetTenantRateLimitPolicyCommand>
{
    public async Task<Result> HandleAsync(SetTenantRateLimitPolicyCommand command, CancellationToken cancellationToken)
    {
        using var scope = userDetailsProvider.CreateSystemUserScope(command.TenantId);

        var tenant = await tenantRepository.FindByIdAsync(
            command.TenantId,
            trackChanges: true,
            includeProperties: t => t.TenantSettings,
            cancellationToken: cancellationToken);

        if (tenant is null)
        {
            throw new NotFoundException($"Tenant '{command.TenantId}' was not found.");
        }

        var rules = command.Rules.Select(rule => new RateLimitRule
        {
            Domain = rule.Domain,
            Scope = rule.Scope,
            Unit = rule.Unit,
            RequestsPerUnit = rule.RequestsPerUnit,
            Burst = rule.Burst
        });

        var policy = new RateLimitPolicy(rules);
        tenant.SetRateLimitPolicy(policy);

        tenantRepository.Update(tenant);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
