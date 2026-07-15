using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.CQRS.Query;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;

namespace EShop.Tenancy.Application.UseCases.Tenants.GetTenantFeatures;

public sealed class GetTenantFeaturesQueryHandler : IQueryHandler<GetTenantFeaturesQuery, Response.TenantFeaturesResponse>
{
    private readonly ITenantFeaturesProvider _provider;

    public GetTenantFeaturesQueryHandler(ITenantFeaturesProvider provider)
    {
        _provider = provider;
    }

    public async Task<Result<Response.TenantFeaturesResponse>> HandleAsync(GetTenantFeaturesQuery query, CancellationToken cancellationToken = default)
    {
        var features = await _provider.GetFeatures(query.TenantId);

        var result = new Response.TenantFeaturesResponse
        {
            FeatureIds = features
        };

        return Result.Success(result);
    }
}
