using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools;
using EShop.Tenancy.Application.Services;
using Microsoft.Extensions.Logging;

namespace EShop.Tenancy.Application.UseCases.V1.Events;

public sealed class UpdateSupportedFeaturesInternalCommand : ICommand
{
    public required string SourceSystemReference { get; init; }

    public IFeature[] Features { get; init; } = [];

    public SupportedFeaturesAction Action { get; init; }

    public required string TenantId { get; init; }

    public required string ActionUserId { get; init; }
}

public class UpdateSupportedFeaturesInternalCommandHandler : ICommandHandler<UpdateSupportedFeaturesInternalCommand>
{
    private readonly IFeatureService _featureService;
    private readonly IResiliencePolicyFactory _resiliencePolicyFactory;
    private readonly ILogger _logger;

    public UpdateSupportedFeaturesInternalCommandHandler(
        IFeatureService featureService,
        IResiliencePolicyFactory resiliencePolicyFactory,
        ILogger<UpdateSupportedFeaturesInternalCommandHandler> logger)
    {
        _featureService = featureService;
        _resiliencePolicyFactory = resiliencePolicyFactory;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(UpdateSupportedFeaturesInternalCommand command, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{Action} {Count} features of source system: {SourceSystemReference}",
            command.Action,
            command.Features.Length,
            command.SourceSystemReference);

        foreach (var feature in command.Features)
        {
            _logger.LogDebug("Processing feature '{Action}' (ID='{Id}')", command.Action, feature.Id);

            var dbFeature = Domain.Entities.Feature.Create(
                feature.Id,
                feature.Name,
                feature.Description,
                feature.Module,
                feature.State);

            await _resiliencePolicyFactory
                .CreateDbUpdateHandlingAsyncPolly(_logger)
                .ExecuteAsync(async () =>
                {
                    if (command.Action == SupportedFeaturesAction.AddOrUpdate)
                    {
                        await _featureService.AddOrUpdateFeatureAsync(dbFeature, cancellationToken);
                    }
                    else
                    {
                        await _featureService.DeleteFeatureAsync(dbFeature, cancellationToken);
                    }
                });
        }

        return Result.Success();
    }
}
