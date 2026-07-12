using EShop.Shared.Contracts.Abstractions.Mediator;
using EShop.Shared.Contracts.Services.Tenancy.Features;

namespace EShop.Tenancy.Application.UseCases.Tenants.GetTenantFeatures;

public sealed class GetTenantFeaturesQuery : IQuery<Response.TenantFeaturesResponse>
{
    public required string TenantId { get; init; }
}
