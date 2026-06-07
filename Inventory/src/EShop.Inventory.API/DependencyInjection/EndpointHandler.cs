using EShop.Inventory.API.APIs;

namespace EShop.Inventory.API.DependencyInjection;

public static class EndpointHandler
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapInventoryEndpoints()
            .MapReservationEndpoints();

        return endpoints;
    }
}
