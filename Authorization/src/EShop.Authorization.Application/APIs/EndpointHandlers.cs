namespace EShop.Authorization.Application.APIs;

public static class EndpointHandlers
{
    public static IEndpointRouteBuilder MapAuthorizationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllers();
        endpoints.MapAuthenticationEndpoints();

        return endpoints;
    }
}
