using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.DomainTools.Extensions;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Tenancy.Application.Abstractions;
using EShop.Tenancy.Domain.Abstractions.Repositories;
using EShop.Tenancy.Domain.Entities;
using EShop.Tenancy.Domain.Enumerations;
using EShop.Tenancy.Domain.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EShop.Tenancy.Application.Services;

public sealed class SystemInitializer(
    ILogger<SystemInitializer> logger,
    IUserDetailsProvider userDetailsProvider,
    IEventBus eventBusGateway,
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork,
    IConfiguration configuration) : ISystemInitializer
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        using var scope = userDetailsProvider.CreateSystemUserScope(UserData.SystemTenantId);

        var existing = await tenantRepository.FindByIdAsync(UserData.SystemTenantId, cancellationToken: cancellationToken);
        if (existing is not null)
            return;

        logger.LogInformation("Starting system initialization...");

        var tenant = CreateSystemTenant();

        tenantRepository.Add(tenant);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await eventBusGateway.PublishAsync(new TenantCreated
        {
            TenantId = tenant.Id,
            TenantName = tenant.Name,
            OwnerUsername = tenant.OwnerUsername.Require(),
            OwnerDisplayName = tenant.Name,
            OwnerEmail = tenant.Email.Require(),
            ActionUserId = userDetailsProvider.AuthenticatedUser.ActionUserId,
            ActionUserType = userDetailsProvider.AuthenticatedUser.ActionUserType
        }, cancellationToken);

        logger.LogInformation("System initialized successfully...");
    }

    private Tenant CreateSystemTenant()
    {
        var systemEmail = GetSystemUserEmail();
        var tenant = Tenant.CreateSystemTenant(
            UserData.SystemTenantId,
            UserData.SystemTenantId,
            UserData.SystemUsername,
            systemEmail);

        tenant.AddDefaultTenantSetting();
        tenant.SetRateLimitPolicy(BuildDefaultRateLimitPolicy());

        return tenant;
    }

    private string GetSystemUserEmail()
    {
        return configuration["SystemUser:Email"] ?? $"{UserData.SystemUsername}@eshop.com";
    }

    private static RateLimitPolicy BuildDefaultRateLimitPolicy()
    {
        return new RateLimitPolicy(
        [
            new RateLimitRule
            {
                Domain = "authorization",
                Scope = RateLimitScope.AnonymousIp,
                Unit = RateLimitUnit.Minute,
                RequestsPerUnit = 5
            }
        ]);
    }
}
