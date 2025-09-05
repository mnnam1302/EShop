using EShop.Configuration.Application.Agencies.GetAgencies;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Configuration.Application.Agencies;

internal static class EndpointHandler
{
    private const string BaseUrl = "api/v{version:apiVersion}/agencies";

    public static IEndpointRouteBuilder MapAgencyEndpoints(this IEndpointRouteBuilder routerBuilder)
    {
        var productEndpointsV1 = routerBuilder
            .NewVersionedApi("Agencies")
            .MapGroup(BaseUrl)
            .HasApiVersion(1)
            .RequireFeatureFilter(FeatureConstants.ConfigurationFeatures.ProductBuilder_FeatureId);

        productEndpointsV1
            .MapGetAgencies();

        return routerBuilder;
    }
}
