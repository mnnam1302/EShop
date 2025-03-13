using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Tenancy.Domain;
using EShop.Tenancy.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Polly;

namespace EShop.Tenancy.Application.UseCases.V1.Events;

public sealed class SupportedFeaturesUpdatedConsumerHandler : ICommandHandler<SupportedFeaturesUpdated>
{
    private readonly IFeatureRepository _featureRepository;
    private readonly ITenancyUnitOfWork _tenancyUnitOfWork;
    private ILogger _logger;

    public SupportedFeaturesUpdatedConsumerHandler(
        IFeatureRepository featureRepository,
        ITenancyUnitOfWork tenancyUnitOfWork,
        ILogger<SupportedFeaturesUpdatedConsumerHandler> logger)
    {
        _featureRepository = featureRepository;
        _tenancyUnitOfWork = tenancyUnitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(SupportedFeaturesUpdated request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{action} {count} features of source system: {sourceSystemReference}",
            request.Action,
            request.Features.Length,
            request.SourceSystemReference);

        foreach (var feature in request.Features)
        {
            _logger.LogDebug("Processing feature '{action}' (ID='{id}')", request.Action, feature.Id);


        }

        return Result.Success();
    }
}