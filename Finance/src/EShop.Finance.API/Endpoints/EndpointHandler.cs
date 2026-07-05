namespace EShop.Finance.API.Endpoints;

public static class EndpointHandler
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder endpoints)
    {
        return endpoints
            .MapAccountEndpoints()
            .MapAccountingCompanyEndpoints();
    }
}

