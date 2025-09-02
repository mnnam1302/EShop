using EShop.Shared.JsonApi.ResourceAccessControl;

namespace EShop.Configuration.Application.Agencies.GetAgencies;

public static class EndpointHandler
{
    public static RouteGroupBuilder MapGetAgencies(this RouteGroupBuilder agencyEndpointBuilder)
    {
        agencyEndpointBuilder.MapPost("/", GetAgenciesAsync)
            .RequirePermissionFilter("");

        return agencyEndpointBuilder;
    }

    private static async Task GetAgenciesAsync()
    {
        throw new NotImplementedException();
    }
}
