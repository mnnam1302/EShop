using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;

namespace EShop.Tenancy.Application.UseCases.V1.Queries.Features;

public class GetFeaturesQueryHandler : IQueryHandler<Query.GetFeaturesQuery, Response.FeatureResponseInternal>
{
    private readonly ITenantFeaturesProvider _tenantFeaturesProvider;
    private readonly IUserDetailsProvider _userDetailsProvider;

    public GetFeaturesQueryHandler(ITenantFeaturesProvider tenantFeaturesProvider, IUserDetailsProvider userDetailsProvider)
    {
        _tenantFeaturesProvider = tenantFeaturesProvider;
        _userDetailsProvider = userDetailsProvider;
    }

    public async Task<Result<Response.FeatureResponseInternal>> Handle(Query.GetFeaturesQuery request, CancellationToken cancellationToken)
    {
        var features = await _tenantFeaturesProvider.GetFeatures(_userDetailsProvider.AuthenticatedUser.TenantId);

        var result = new Response.FeatureResponseInternal
        {
            FeatureIds = features
        };

        return Result.Success(result);
    }
}