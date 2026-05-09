using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;

namespace EShop.Inventory.API.APIs;

public static class InventoryApis
{
    private const string BaseUrl = "api/v{version:apiVersion}/inventories";

    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder routerBuilder)
    {
        var inventoryEndpointsV1 = routerBuilder
            .NewVersionedApi("Inventory")
            .MapGroup(BaseUrl)
            .HasApiVersion(1)
            .RequireFeatureFilter(FeatureConstants.Catalog.Product_FeatureId);

        inventoryEndpointsV1.MapPost("", CreateInventoriesV1Async);

        return routerBuilder;
    }

    private static async Task CreateInventoriesV1Async()
    {
        throw new NotImplementedException();
    }
}
