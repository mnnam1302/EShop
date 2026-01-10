using EShop.Shared.Contracts.Abstractions.Requests;

namespace EShop.Shared.Contracts.Services.Tenancy.Features;

public static class Query
{
    public sealed class GetTenantFeaturesQuery(string tenantId) : IQuery<Response.TenantFeaturesResponse>
    {
        public string TenantId { get; init; } = tenantId;
    }
}