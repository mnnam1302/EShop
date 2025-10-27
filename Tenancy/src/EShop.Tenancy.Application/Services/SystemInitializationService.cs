using EShop.Shared.Authentication;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.EventBus.Services;
using EShop.Tenancy.Domain.Entities;
using EShop.Tenancy.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EShop.Tenancy.Application.Services;

internal sealed class SystemInitializationService : BackgroundService
{
    private readonly ILogger<SystemInitializationService> _logger;
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly IEventBusGateway _eventBusGateway;
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public SystemInitializationService(
        ILogger<SystemInitializationService> logger,
        IUserDetailsProvider userDetailsProvider,
        IEventBusGateway eventBusGateway,
        ITenantRepository tenantRepository,
        IConfiguration configuration,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _userDetailsProvider = userDetailsProvider;
        _eventBusGateway = eventBusGateway;
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _userDetailsProvider.SetSystemUserContext(UserData.SystemTenantId);

            _logger.LogInformation("Starting system initialization...");

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            if (await IsSystemInitialized(stoppingToken))
                return;

            _logger.LogInformation("System not initialized yet, creating Super Admin...");

            var systemTenant = await InitializeSystemTenantAsync(stoppingToken);

            await _eventBusGateway.PublishAsync<ITenantCreated>(new
            {
                TenantId = systemTenant.Id,
                TenantName = systemTenant.Name,
                OwnerUsername = systemTenant.OwnerUsername,
                OwnerDisplayName = systemTenant.Name,
                OwnerEmail = systemTenant.Email,
                ActionUserId = _userDetailsProvider.AuthenticatedUser.ActionUserId,
                ActionUserType = _userDetailsProvider.AuthenticatedUser.ActionUserType
            }, stoppingToken);

            _logger.LogInformation("Super Admin initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize system");
        }
        finally
        {
            _userDetailsProvider.ClearSystemUserContext();
        }
    }

    private async Task<bool> IsSystemInitialized(CancellationToken cancellationToken)
    {
        var systemTenant = await _tenantRepository.FindByIdAsync(UserData.SystemTenantId, cancellationToken: cancellationToken);

        if (systemTenant is not null)
            return true;

        return false;
    }

    private async Task<Tenant> InitializeSystemTenantAsync(CancellationToken cancellationToken)
    {
        var systemEmail = GetSystemUserEmail();
        var tenant = Tenant.CreateSystemTenant(
            UserData.SystemTenantId,
            UserData.SystemTenantId,
            UserData.SystemUsername,
            systemEmail);

        _tenantRepository.Add(tenant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return tenant;
    }

    private string GetSystemUserEmail()
    {
        return _configuration["SystemUser:Email"] ?? $"{UserData.SystemUsername}@eshop.com";
    }
}
