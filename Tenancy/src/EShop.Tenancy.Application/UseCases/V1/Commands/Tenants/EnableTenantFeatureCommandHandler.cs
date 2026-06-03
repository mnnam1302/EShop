using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Mediator;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.Exceptions;
using EShop.Shared.DomainTools.UnitOfWorks;
using EShop.Tenancy.Domain.Repositories;

namespace EShop.Tenancy.Application.UseCases.V1.Commands.Tenants;

public sealed class EnableTenantFeatureCommand : ICommand
{
    public required string TenantId { get; init; }
    public required string FeatureId { get; init; }
}

internal sealed class EnableTenantFeatureCommandHandler : ICommandHandler<EnableTenantFeatureCommand>
{
    private readonly IUserDetailsProvider _userDetailsProvider;
    private readonly ITenantFeatureRepository _tenantFeatureRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;

    public EnableTenantFeatureCommandHandler(
        IUserDetailsProvider userDetailsProvider,
        ITenantFeatureRepository tenantFeatureRepository,
        IEventBus eventBus,
        IUnitOfWork unitOfWork)
    {
        _userDetailsProvider = userDetailsProvider;
        _tenantFeatureRepository = tenantFeatureRepository;
        _eventBus = eventBus;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(EnableTenantFeatureCommand command, CancellationToken cancellationToken)
    {
        using var scope = _userDetailsProvider.CreateSystemUserScope(command.TenantId);

        var tenantFeature = await _tenantFeatureRepository.FindSingleAsync(
            predicate: tf => tf.FeatureId == command.FeatureId,
            trackChanges: true,
            cancellationToken: cancellationToken);

        if (tenantFeature is null)
        {
            throw new BadRequestException("Feature is not found for the tenant.");
        }

        if (tenantFeature.IsEnabled())
        {
            throw new BadRequestException("Feature is already enabled for the tenant.");
        }

        tenantFeature.Enable();

        _tenantFeatureRepository.Update(tenantFeature);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(new TenantFeaturesUpdated
        {
            TenantId = command.TenantId,
            ActionUserId = _userDetailsProvider.AuthenticatedUser.ActionUserId,
            ActionUserType = _userDetailsProvider.AuthenticatedUser.ActionUserType,
        }, cancellationToken);

        return Result.Success();
    }
}