using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.DomainTools.Exceptions;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.EventBus.Abstractions;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Tenancy.Domain.Entities;
using EShop.Tenancy.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShop.Tenancy.Application.Services;

public interface IFeatureService
{
    Task<IEnumerable<TenantFeature>> GetTenantFeaturesByTenantIdAsync(string tenantId, string? state = null, CancellationToken cancellationToken = default);

    Task AddOrUpdateFeatureAsync(Feature feature, CancellationToken cancellationToken);

    Task DeleteFeatureAsync(Feature feature, CancellationToken cancellationToken);
}

public sealed class FeatureService : IFeatureService
{
    private readonly IFeatureRepository _featureRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly IEventBus _eventBusGateway;
    private readonly ILogger _logger;

    public FeatureService(
        IFeatureRepository featureRepository,
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork,
        IUserDetailsProvider userDetailsProvider,
        IEventBus eventBusGateway,
        ILogger<FeatureService> logger)
    {
        _featureRepository = featureRepository;
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _userDetailsProvider = userDetailsProvider;
        _eventBusGateway = eventBusGateway;
        _logger = logger;
    }

    public async Task AddOrUpdateFeatureAsync(Feature feature, CancellationToken cancellationToken)
    {
        var entityState = await AddOrUpdateFeatureInternalAsync(feature, cancellationToken);
        if (entityState == EntityState.Added)
        {
            await RegisterTenantFeature(feature, cancellationToken);
        }
    }

    private async Task<EntityState> AddOrUpdateFeatureInternalAsync(Feature feature, CancellationToken cancellationToken)
    {
        feature.Category ??= FeatureCategory.Permanent.ToString();

        var existingFeature = await _featureRepository.FindByIdAsync(feature.Id, cancellationToken: cancellationToken);

        if (existingFeature != null)
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
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Feature {FeatureId} added to system", feature.Id);
        return entityState;
    }

    private async Task RegisterTenantFeature(Feature feature, CancellationToken cancellationToken)
    {
        var tenantIds = await _tenantRepository
            .FindAll()
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        foreach (var tenantId in tenantIds)
        {
            try
            {
                _userDetailsProvider.SetSystemUserContext(tenantId);

                var tenant = await _tenantRepository.FindByIdAsync(
                    tenantId,
                    trackChanges: true,
                    cancellationToken: cancellationToken,
                    includeProperties: t => t.TenantFeatures);
                if (tenant != null)
                {
                    tenant.AddTenantFeature(feature.Id, feature.DefaultStateForNewTenant, _userDetailsProvider.AuthenticatedUser.ActionUserId);

                    _tenantRepository.Update(tenant);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    await PublishTenantFeaturesUpdatedAsync(tenantId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register TenantFeature error - tenant: '{TenantId}', feature: '{FeatureId}'", tenantId, feature.Id);
            }
            finally
            {
                _userDetailsProvider.ClearSystemUserContext();
            }
        }
    }

    private async Task PublishTenantFeaturesUpdatedAsync(string tenantId)
    {
        await _eventBusGateway.PublishAsync<ITenantFeaturesUpdated>(new
        {
            EventId = Guid.NewGuid(),
            TenantId = tenantId,
            ActionUserId = _userDetailsProvider.AuthenticatedUser.ActionUserId,
            ActionUserType = _userDetailsProvider.AuthenticatedUser.ActionUserType,
        });
    }

    public async Task DeleteFeatureAsync(Feature feature, CancellationToken cancellationToken)
    {
        await DeleteAsync(feature.Id, cancellationToken);
    }

    public async Task DeleteAsync(string featureId, CancellationToken cancellationToken)
    {
        await AssertNoTenantUsingFeature(featureId, cancellationToken);

        try
        {
            var tenantIds = await _tenantRepository.FindAll().Select(t => t.Id).ToListAsync(cancellationToken);

            foreach (var tenantId in tenantIds)
            {
                _userDetailsProvider.SetSystemUserContext(tenantId);
                try
                {
                    var tenant = await _tenantRepository.FindByIdAsync(tenantId, cancellationToken: cancellationToken, includeProperties: t => t.TenantFeatures);
                    if (tenant != null)
                    {
                        tenant.RemoveTenantFeature(featureId);

                        _tenantRepository.Update(tenant);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        await PublishTenantFeaturesUpdatedAsync(tenantId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Delete TenantFeature error - tenant: '{TenantId}', feature: '{FeatureId}'", tenantId, featureId);
                }
                finally
                {
                    _userDetailsProvider.ClearSystemUserContext();
                }
            }

            var deletedFeature = await _featureRepository.FindSingleAsync(x => x.Id == featureId, cancellationToken: cancellationToken);
            if (deletedFeature is not null)
            {
                _featureRepository.Delete(deletedFeature);
                await _unitOfWork.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Delete system feature error - feature: '{FeatureId}'", featureId);
        }
        finally
        {
            _userDetailsProvider.ClearSystemUserContext();
        }
    }

    private async Task AssertNoTenantUsingFeature(string featureId, CancellationToken cancellationToken)
    {
        var enableSystemFeature = await _featureRepository.FindSingleAsync(x => x.Id == featureId && x.State == FeatureState.Enabled.ToString());
        if (enableSystemFeature is not null)
        {
            throw new UnprocessableEntityException($"Cannot delete system feature id {featureId} because it is enabled");
        }

        // Retrieve all tenant IDs before setting each tenant ID for the system user context. This is necessary because tenant feature entities are related to RSL security.
        var tenantIds = await _tenantRepository.FindAll().Select(t => t.Id).ToListAsync(cancellationToken);
        if (tenantIds.Count == 0)
        {
            return;
        }

        foreach (var tenantId in tenantIds)
        {
            _userDetailsProvider.SetSystemUserContext(tenantId);
            try
            {
                var tenantUsingFeature = await _tenantRepository.FindByConditionAsync(
                    x => x.Id == tenantId && x.TenantFeatures.Any(
                        tf => tf.FeatureId == featureId && tf.State == FeatureState.Enabled.ToString()),
                    cancellationToken: cancellationToken);

                if (tenantUsingFeature != null)
                {
                    throw new UnprocessableEntityException($"Cannot delete feature id {featureId} because tenant {tenantId} is still using it");
                }
            }
            finally
            {
                _userDetailsProvider.ClearSystemUserContext();
            }
        }
    }

    public async Task<IEnumerable<TenantFeature>> GetTenantFeaturesByTenantIdAsync(string tenantId, string? state = null, CancellationToken cancellationToken = default)
    {
        try
        {
            tenantId = tenantId.ToLower();
            _userDetailsProvider.SetSystemUserContext(tenantId);

            var query = _tenantRepository.FindAll(false, t => t.TenantFeatures)
                .Where(t => t.Id == tenantId)
                .SelectMany(t => t.TenantFeatures);

            if (!string.IsNullOrEmpty(state))
            {
                query = query.Where(tf => tf.State == state);
            }

            var tenantFeatures = await query
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Get features for tenant '{Id}' stored in the database. Result: {Count} features available", tenantId, tenantFeatures.Count);

            return tenantFeatures;
        }
        finally
        {
            _userDetailsProvider.ClearSystemUserContext();
        }
    }
}