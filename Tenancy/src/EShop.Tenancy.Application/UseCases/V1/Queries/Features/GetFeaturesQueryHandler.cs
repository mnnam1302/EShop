using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Tenancy.Domain.Repositories;

namespace EShop.Tenancy.Application.UseCases.V1.Queries.Features;

public class GetFeaturesQueryHandler : IQueryHandler<Query.GetFeaturesQuery, List<Response.FeatureResponseInternal>>
{
    private readonly IFeatureRepository _featureRepository;

    public GetFeaturesQueryHandler(IFeatureRepository featureRepository)
    {
        _featureRepository = featureRepository;
    }

    public Task<Result<List<Response.FeatureResponseInternal>>> Handle(Query.GetFeaturesQuery request, CancellationToken cancellationToken)
    {
        // Need implmented get feature owner service tenancy.
        
    }
}