using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.EventBus.Services;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.Exceptions;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Tenancy.Domain;
using EShop.Tenancy.Domain.Entities;
using EShop.Tenancy.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EShop.Tenancy.Application.UseCases.V1.Commands.Tenants;

public class CreateTenantCommandHandler : ICommandHandler<Shared.Contracts.Services.Tenancy.Tenants.Command.CreateTenantCommand>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenancyUnitOfWork _tenancyUnitOfWork;
    private readonly IEventBusGateway _eventBusGateway;
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly IFeatureRepository _featureRepository;

    public CreateTenantCommandHandler(
        ITenantRepository tenantRepository,
        ITenancyUnitOfWork tenancyUnitOfWork,
        IEventBusGateway eventBusGateway,
        IUserDetailsProvider userDetailsProvider,
        IFeatureRepository featureRepository)
    {
        _tenantRepository = tenantRepository;
        _tenancyUnitOfWork = tenancyUnitOfWork;
        _eventBusGateway = eventBusGateway;
        _userDetailsProvider = userDetailsProvider;
        _featureRepository = featureRepository;
    }

    public async Task<Result> Handle(Shared.Contracts.Services.Tenancy.Tenants.Command.CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var existsingTenant = await _tenantRepository.FindSingleAsync(x => x.Name == request.Name);
        if (existsingTenant is not null)
        {
            throw new UnprocessableEntityException("Tenant with the same name already exists.");
        }

        var operationalUser = _userDetailsProvider.AuthenticatedUser;

        var tenant = Tenant.Create(request);
        await EnsureTenantAvaibleFeatures(tenant, operationalUser.ActionUserId);

        await _eventBusGateway.PublishAsync<IntegrationEvent.TenantCreated>(new
        {
            TenantId = tenant.Id,
            TenantName = tenant.Name,
            OwnerUsername = tenant.OwnerUsername,
            OwnerDisplayName = tenant.Name ?? Tenant.RemoveDomainSuffix(request.OwnerUsername, tenant.Id),
            OwnerEmail = tenant.Email,
            ActionUserId = operationalUser.ActionUserId,
            ActionUserType = operationalUser.ActionUserType
        }, cancellationToken);

        return Result.Success();
    }

    private async Task EnsureTenantAvaibleFeatures(Tenant tenant, string operationalUsername)
    {
        try
        {
            _userDetailsProvider.SetSystemUserContext(tenant.Id);

            var features = await _featureRepository.FindAll().ToListAsync();

            foreach (var feature in features)
            {
                tenant.AddTenantFeature(feature.Id, feature.DefaultStateForNewTenant ?? FeatureConstants.InitialState, operationalUsername);
            }

            _tenantRepository.Add(tenant);
            await _tenancyUnitOfWork.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new UnprocessableEntityException(ex.Message);
        }
        finally
        {
            _userDetailsProvider.ClearSystemUserContext();
        }
    }
}