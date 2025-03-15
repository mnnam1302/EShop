using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.DomainTools;
using EShop.Tenancy.Application.Services;
using Microsoft.Extensions.Logging;
using static EShop.Shared.Contracts.Services.Tenancy.Features.Command;

namespace EShop.Tenancy.Application.UseCases.V1.Events;

public sealed class SupportedFeaturesUpdatedConsumerHandler : ICommandHandler<UpdateSupportFeaturesCommand>
{
    private readonly IFeatureService _featureService;
    private readonly IResiliencePolicyFactory _resiliencePolicyFactory;
    private ILogger _logger;

    public SupportedFeaturesUpdatedConsumerHandler(
        IFeatureService featureService,
        IResiliencePolicyFactory resiliencePolicyFactory,
        ILogger<SupportedFeaturesUpdatedConsumerHandler> logger)
    {
        _featureService = featureService;
        _resiliencePolicyFactory = resiliencePolicyFactory;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateSupportFeaturesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{action} {count} features of source system: {sourceSystemReference}",
            request.Action,
            request.Features.Length,
            request.SourceSystemReference);

        foreach (var feature in request.Features)
        {
            _logger.LogDebug("Processing feature '{action}' (ID='{id}')", request.Action, feature.Id);

            var dbFeature = new Domain.Entities.Feature
            {
                Id = feature.Id,
                Name = feature.Name,
                Description = feature.Description,
                Module = feature.Module,
                State = feature.State,

                // Adding DefaultStateForNewTenant to Feature interface
                // will cause changes to all microservices. Assuming that DefaultStateForNewTenant
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