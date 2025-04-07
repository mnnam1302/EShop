using EShop.Shared.Contracts.Abstractions.Requests;

namespace EShop.Shared.Contracts.Services.Tenancy.Features;

public static class Query
{
    public record GetFeaturesQuery() : IQuery<Response.FeatureResponseInternal>;
}