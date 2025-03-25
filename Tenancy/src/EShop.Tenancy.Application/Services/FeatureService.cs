using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Tenancy.Application.Abstrations;
using EShop.Tenancy.Domain;
using EShop.Tenancy.Domain.Entities;
using EShop.Tenancy.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Tenancy.Application.Services;

public interface IFeatureService
{
    Task AddOrUpdateFeatureAsync(Feature feature, string? state, CancellationToken cancellationToken);

    Task DeleteFeatureAsync(Feature feature, CancellationToken cancellationToken);
}

public class FeatureService : IFeatureService
{
    private readonly IFeatureRepository _featureRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenancyUnitOfWork _unitOfWork;
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly IEventBusGateway _eventBusGateway;
    private readonly ILogger _logger;

    public FeatureService(
        IFeatureRepository featureRepository,
        ITenantRepository tenantRepository,
        ITenancyUnitOfWork unitOfWork,
        IUserDetailsProvider userDetailsProvider,
        IEventBusGateway eventBusGateway,
        ILogger<FeatureService> logger)
    {
        _featureRepository = featureRepository;
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _userDetailsProvider = userDetailsProvider;
        _eventBusGateway = eventBusGateway;
        _logger = logger;
    }

    public async Task AddOrUpdateFeatureAsync(Feature feature, string? state, CancellationToken cancellationToken)
    {
        var entityState = await AddOrUpdateFeatureInternalAsync(feature, cancellationToken);
        if (entityState == EntityState.Added)
        {
            await RegisterTenantFeature(feature, state, cancellationToken);
        }
    }

    private async Task<EntityState> AddOrUpdateFeatureInternalAsync(Feature feature, CancellationToken cancellationToken)
    {
        feature.Category ??= FeatureCategory.Permanent.ToString();

        var existingFeature = await _featureRepository.FindByIdAsync(feature.Id, cancellationToken: cancellationToken);

        if (existingFeature is not null)
        {
            feature.State = existingFeature.State;
            feature.DefaultStateForNewTenant = existingFeature.DefaultStateForNewTenant;

            _featureRepository.Update(feature);
        }
        else
        {
            _featureRepository.Add(feature);
        }

        var entityState = await _featureRepository.GetEntityStateAsync(feature, cancellationToken);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogTrace("Feature '{featureId}' added to system", feature.Id);
        return entityState;
    }

    private async Task RegisterTenantFeature(Feature feature, string? state, CancellationToken cancellationToken)
    {
        var tenants = await _tenantRepository.FindAll(trackChanges: true).ToListAsync(cancellationToken);
        foreach (var tenant in tenants)
        {
            _userDetailsProvider.SetSystemUserContext(tenant.Id);
            try
            {
                tenant.ConfigureFeature(feature.Id, state ?? feature.DefaultStateForNewTenant, _userDetailsProvider.AuthenticatedUser.ActionUserId);

                _tenantRepository.Update(tenant);
                await _unitOfWork.SaveChangesAsync();

                await _eventBusGateway.PublishAsync<TenantFeaturesUpdated>(new
                {
                    EventId = Guid.NewGuid(),
                    TenantId = tenant.Id,
                    ActionUserId = _userDetailsProvider.AuthenticatedUser.ActionUserId,
                    ActionUserType = _userDetailsProvider.AuthenticatedUser.ActionUserType,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register TenantFeature error - tenant: '{tenantId}', feature: '{featureId}'", tenant.Id, feature.Id);
            }
            finally
            {
                _userDetailsProvider.ClearSystemUserContext();
            }
        }
    }

    public async Task DeleteFeatureAsync(Feature feature, CancellationToken cancellationToken)
    {
        //var command
    }
}