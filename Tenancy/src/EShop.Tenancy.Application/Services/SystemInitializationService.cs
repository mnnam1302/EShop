using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.EventBus.Abstractions;
using EShop.Tenancy.Application.Abstractions;
using EShop.Tenancy.Domain.Entities;
using EShop.Tenancy.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EShop.Tenancy.Application.Services;

public sealed class SystemInitializationService(
    ILogger<SystemInitializationService> logger,
    IUserDetailsProvider userDetailsProvider,
    IEventBusGateway eventBusGateway,
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork,
    IConfiguration configuration) : ISystemInitializationService
{
    public async Task InitializeSystemAsync(CancellationToken cancellationToken = default)
    {
        userDetailsProvider.SetSystemUserContext(UserData.SystemTenantId);

        try
        {
            if (await IsSystemInitializedAsync(cancellationToken))
                return;

            logger.LogInformation("System not initialized yet, creating Super Admin...");

            var systemTenant = await CreateSystemTenantAsync(cancellationToken);

            await PublishTenantCreatedEventAsync(systemTenant, cancellationToken);

            logger.LogInformation("Super Admin initialized successfully");
        }
        finally
        {
            userDetailsProvider.ClearSystemUserContext();
        }
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
        await eventBusGateway.PublishAsync<ITenantCreated>(new
        {
            TenantId = systemTenant.Id,
            TenantName = systemTenant.Name,
            OwnerUsername = systemTenant.OwnerUsername,
            OwnerDisplayName = systemTenant.Name,
            OwnerEmail = systemTenant.Email,
            ActionUserId = userDetailsProvider.AuthenticatedUser.ActionUserId,
            ActionUserType = userDetailsProvider.AuthenticatedUser.ActionUserType
        }, cancellationToken);
    }
}