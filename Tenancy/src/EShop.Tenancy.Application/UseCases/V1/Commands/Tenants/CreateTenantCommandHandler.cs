using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.DomainTools.Exceptions;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.EventBus.Services;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Tenancy.Domain.Entities;
using EShop.Tenancy.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EShop.Tenancy.Application.UseCases.V1.Commands.Tenants;

internal sealed class CreateTenantCommandHandler(
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork,
    IEventBusGateway eventBusGateway,
    IUserDetailsProvider userDetailsProvider,
    IFeatureRepository featureRepository) : ICommandHandler<Command.CreateTenantCommand>
{
    public async Task<Result> Handle(Command.CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var existingTenant = await tenantRepository.FindSingleAsync(x => x.Id == request.Id || x.Name == request.Name);
        if (existingTenant is not null)
        {
            throw new BadRequestException($"Tenant with ID {request.Id} or name {request.Name} has already exists.");
        }

        var operationalUser = userDetailsProvider.AuthenticatedUser;

        var tenant = Tenant.Create(request);
        tenant.AddDefaultTenantSetting();

        // TODO: can implement domain event handler to handle tenant features
        await EnsureTenantAvailableFeatures(tenant, operationalUser.ActionUserId, cancellationToken);

        await eventBusGateway.PublishAsync<ITenantCreated>(new
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

    private async Task EnsureTenantAvailableFeatures(Tenant tenant, string operationalUsername, CancellationToken cancellationToken)
    {
        userDetailsProvider.SetSystemUserContext(tenant.Id);

        try
        {
            var features = await featureRepository.FindAll().ToListAsync(cancellationToken);
            foreach (var feature in features)
            {
                tenant.AddTenantFeature(feature.Id, feature.DefaultStateForNewTenant ?? FeatureIds.InitialState, operationalUsername);
            }

            tenantRepository.Add(tenant);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new BadRequestException(ex.Message);
        }
        finally
        {
            userDetailsProvider.ClearSystemUserContext();
        }
    }
}