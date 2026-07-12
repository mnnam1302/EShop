using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.DomainTools.Extensions;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Tenancy.Application.Abstractions;
using EShop.Tenancy.Domain.Abstractions.Repositories;
using EShop.Tenancy.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EShop.Tenancy.Application.Services;

public sealed class SystemInitializationService(
    ILogger<SystemInitializationService> logger,
    IUserDetailsProvider userDetailsProvider,
    IEventBus eventBusGateway,
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork,
    IConfiguration configuration) : ISystemInitializationService
{
    public async Task InitializeSystemAsync(CancellationToken cancellationToken = default)
    {
        using var scope = userDetailsProvider.CreateSystemUserScope(UserData.SystemTenantId);

        if (await IsSystemInitializedAsync(cancellationToken))
        {
            return;
        }

        logger.LogInformation("System not initialized yet, creating Super Admin...");

        var systemTenant = await CreateSystemTenantAsync(cancellationToken);

        await PublishTenantCreatedEventAsync(systemTenant, cancellationToken);

        logger.LogInformation("Super Admin initialized successfully");
    }

    private async Task<bool> IsSystemInitializedAsync(CancellationToken cancellationToken = default)
    {
        var systemTenant = await tenantRepository.FindByIdAsync(UserData.SystemTenantId, cancellationToken: cancellationToken);
        return systemTenant is not null;
    }

    private async Task<Tenant> CreateSystemTenantAsync(CancellationToken cancellationToken)
    {
        var systemEmail = GetSystemUserEmail();
        var tenant = Tenant.CreateSystemTenant(
            UserData.SystemTenantId,
            UserData.SystemTenantId,
            UserData.SystemUsername,
            systemEmail);

        tenantRepository.Add(tenant);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return tenant;
    }

    private string GetSystemUserEmail()
    {
        return configuration["SystemUser:Email"] ?? $"{UserData.SystemUsername}@eshop.com";
    }

    private async Task PublishTenantCreatedEventAsync(Tenant systemTenant, CancellationToken cancellationToken)
    {
        await eventBusGateway.PublishAsync(new TenantCreated
        {
            TenantId = systemTenant.Id,
            TenantName = systemTenant.Name,
            OwnerUsername = systemTenant.OwnerUsername.Require(),
            OwnerDisplayName = systemTenant.Name,
            OwnerEmail = systemTenant.Email.Require(),
            ActionUserId = userDetailsProvider.AuthenticatedUser.ActionUserId,
            ActionUserType = userDetailsProvider.AuthenticatedUser.ActionUserType
        }, cancellationToken);
    }
}