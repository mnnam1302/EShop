using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.Exceptions;
using EShop.Shared.DomainTools.Extensions;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Shared.Scoping.ResourceAccessControl;
using EShop.Tenancy.Domain.Abstractions.Repositories;
using EShop.Tenancy.Domain.Commands;
using EShop.Tenancy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EShop.Tenancy.Application.UseCases.Tenants.CreateTenant;

internal sealed class CreateTenantCommandHandler(
    ITenantRepository tenantRepository,
    IUnitOfWork unitOfWork,
    IEventBus eventBusGateway,
    IUserDetailsProvider userDetailsProvider,
    IFeatureRepository featureRepository) : ICommandHandler<CreateTenantCommand>
{
    public async Task<Result> HandleAsync(CreateTenantCommand command, CancellationToken cancellationToken)
    {
        var existingTenant = await tenantRepository.FindSingleAsync(x => x.Id == command.Id || x.Name == command.Name);
        if (existingTenant is not null)
        {
            throw new BadRequestException($"Tenant with ID {command.Id} or name {command.Name} has already exists.");
        }

        var operationalUser = userDetailsProvider.AuthenticatedUser;

        var tenant = Tenant.Create(command);
        tenant.AddDefaultTenantSetting();

        await EnsureTenantAvailableFeatures(tenant, operationalUser.ActionUserId, cancellationToken);

        await eventBusGateway.PublishAsync(new TenantCreated
        {
            TenantId = tenant.Id,
            TenantName = tenant.Name,
            OwnerUsername = tenant.OwnerUsername.Require(),
            OwnerDisplayName = tenant.Name ?? Tenant.RemoveDomainSuffix(command.OwnerUsername, tenant.Id),
            OwnerEmail = tenant.Email.Require(),
            ActionUserId = operationalUser.ActionUserId,
            ActionUserType = operationalUser.ActionUserType
        }, cancellationToken);

        return Result.Success();
    }

    private async Task EnsureTenantAvailableFeatures(Tenant tenant, string operationalUsername, CancellationToken cancellationToken)
    {
        using var scope = userDetailsProvider.CreateSystemUserScope(tenant.Id);

        try
        {
            var features = await featureRepository.FindAll().ToListAsync(cancellationToken);
            foreach (var feature in features)
            {
                tenant.AddTenantFeature(feature.Id, feature.DefaultStateForNewTenant ?? FeatureConstants.InitialState, operationalUsername);
            }

            tenantRepository.Add(tenant);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new BadRequestException(ex.Message);
        }
    }
}
