using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.DomainTools;
using EShop.Tenancy.Application.Services;
using Microsoft.Extensions.Logging;
using static EShop.Shared.Contracts.Services.Tenancy.Features.Command;

namespace EShop.Tenancy.Application.UseCases.V1.Events;

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

    public async Task<Result> Handle(UpdateSupportedFeaturesInternalCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{Action} {Count} features of source system: {SourceSystemReference}",
            request.Action,
            request.Features.Length,
            request.SourceSystemReference);

        foreach (var feature in request.Features)
        {
            _logger.LogDebug("Processing feature '{Action}' (ID='{Id}')", request.Action, feature.Id);

            var dbFeature = new Domain.Entities.Feature
            {
                Id = feature.Id,
                Name = feature.Name,
                Description = feature.Description,
                Module = feature.Module,
                State = feature.State,

                // Adding DefaultStateForNewTenant to Feature interface will cause changes to all microservices. Assuming that DefaultStateForNewTenant
                // should be initialized with the same value as State.
                DefaultStateForNewTenant = feature.State
            };

            await _resiliencePolicyFactory
                .CreateDbUpdateHandlingAsyncPolly(_logger)
                .ExecuteAsync(async () =>
                {
                    if (request.Action == SupportedFeaturesAction.AddOrUpdate)
                    {
                        await _featureService.AddOrUpdateFeatureAsync(dbFeature, feature.State, cancellationToken);
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