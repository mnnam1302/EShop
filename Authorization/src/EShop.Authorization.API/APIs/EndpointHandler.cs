namespace EShop.Authorization.API.APIs;

public static class EndpointHandler
{
    public static IEndpointRouteBuilder MapAuthorizationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapAuthEndpoints()
            .MapUserEndpoints()
            .MapOrganizationEndpoints()
            .MapRoleEndpoints();

        return endpoints;
    }
}
