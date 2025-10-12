namespace EShop.Authorization.API.APIs;

public static class EndpointHandler
{
    public static IEndpointRouteBuilder MapAuthorizationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllers();
        endpoints.MapAuthenticationEndpoints();

        return endpoints;
    }
}
