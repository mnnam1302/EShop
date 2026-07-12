using EShop.Shared.Contracts.Abstractions.Mediator;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Tenancy.Application.Services;
using EShop.Tenancy.Domain.Entities;

namespace EShop.Tenancy.Application.UseCases.Commands.Features;

public sealed class CreateSystemFeatureCommand : ICommand
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string State { get; init; }
    public string Module { get; init; } = string.Empty;
}

internal sealed class CreateSystemFeatureCommandHandler : ICommandHandler<CreateSystemFeatureCommand>
{
    private readonly IFeatureService featureService;

    public CreateSystemFeatureCommandHandler(IFeatureService featureService)
    {
        this.featureService = featureService;
    }

    public async Task<Result> HandleAsync(CreateSystemFeatureCommand command, CancellationToken cancellationToken)
    {
        var feature = Feature.Create(
            command.Id,
            command.Name,
            command.Description,
            command.Module,
            command.State
        );

        await featureService.AddOrUpdateFeatureAsync(feature, cancellationToken);

        return Result.Success();
    }
}