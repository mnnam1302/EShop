using EShop.Shared.Contracts.Abstractions.Requests;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;

namespace EShop.Tenancy.Application.UseCases.V1.Queries.Tenants;

public sealed class GetTenantFeaturesQueryHandler : IQueryHandler<Query.GetTenantFeaturesQuery, Response.TenantFeaturesResponse>
{
    private readonly ITenantFeaturesProvider _tenantFeaturesProvider;

    public GetTenantFeaturesQueryHandler(ITenantFeaturesProvider tenantFeaturesProvider)
    {
        _tenantFeaturesProvider = tenantFeaturesProvider;
    }

    public async Task<Result<Response.TenantFeaturesResponse>> Handle(Query.GetTenantFeaturesQuery request, CancellationToken cancellationToken)
    {
        var features = await _tenantFeaturesProvider.GetFeatures(request.TenantId);

        var result = new Response.TenantFeaturesResponse
        {
            FeatureIds = features
        };

        return Result.Success(result);
    }
}