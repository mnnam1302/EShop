using EShop.Shared.Contracts.Abstractions.Mediator;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Tenancy.Domain.Entities;

namespace EShop.Tenancy.Application.UseCases.Features.CreateFeature;

public sealed class CreateFeatureCommand : ICommand
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string State { get; init; }
    public string Module { get; init; } = string.Empty;
}

internal sealed class CreateFeatureCommandHandler : ICommandHandler<CreateFeatureCommand>
{
    private readonly IFeatureService featureService;

    public CreateFeatureCommandHandler(IFeatureService featureService)
    {
        this.featureService = featureService;
    }

    public async Task<Result> HandleAsync(CreateFeatureCommand command, CancellationToken cancellationToken)
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
