using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Tenancy.Features;
using EShop.Shared.CQRS.Query;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.TenantFeaturesProvider;

namespace EShop.Tenancy.Application.UseCases.V1.Queries.Tenants;

public sealed class GetTenantFeaturesQuery : IQuery<Response.TenantFeaturesResponse>
{
    public required string TenantId { get; init; }
}

public sealed class GetTenantFeaturesQueryHandler : IQueryHandler<GetTenantFeaturesQuery, Response.TenantFeaturesResponse>
{
    private readonly ITenantFeaturesProvider _tenantFeaturesProvider;

    public GetTenantFeaturesQueryHandler(ITenantFeaturesProvider tenantFeaturesProvider)
    {
        _tenantFeaturesProvider = tenantFeaturesProvider;
    }

    public async Task<Result<Response.TenantFeaturesResponse>> HandleAsync(GetTenantFeaturesQuery query, CancellationToken cancellationToken = default)
    {
        var features = await _tenantFeaturesProvider.GetFeatures(query.TenantId);

        var result = new Response.TenantFeaturesResponse
        {
            FeatureIds = features
        };

        return Result.Success(result);
    }
}
