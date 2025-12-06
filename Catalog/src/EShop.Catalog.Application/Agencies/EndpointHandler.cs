using EShop.Catalog.Application.Agencies.GetAgencies;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Catalog.Application.Agencies;

public static class EndpointHandler
{
    private const string BaseUrl = "api/v{version:apiVersion}/agencies";

    public static IEndpointRouteBuilder MapAgencyEndpoints(this IEndpointRouteBuilder routerBuilder)
    {
        var productEndpointsV1 = routerBuilder
            .NewVersionedApi("Agency")
            .MapGroup(BaseUrl)
            .HasApiVersion(1)
            .RequireFeatureFilter(FeatureConstants.Catalog.ProductBuilder_FeatureId);

        productEndpointsV1
            .MapGetAgencies();

        return routerBuilder;
    }
}
