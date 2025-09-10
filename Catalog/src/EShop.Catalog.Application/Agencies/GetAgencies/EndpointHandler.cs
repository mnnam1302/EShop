using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.ResourceAccessControl;
using static EShop.Shared.Scoping.ResourceAccessControl.PermissionConstants;

namespace EShop.Catalog.Application.Agencies.GetAgencies;

public static class EndpointHandler
{
    public static RouteGroupBuilder MapGetAgencies(this RouteGroupBuilder agencyEndpointBuilder)
    {
        agencyEndpointBuilder.MapGet("/", GetAgenciesAsync)
            .RequirePermissionFilter(CatalogPermissions.ViewProductsPermissionId);

        return agencyEndpointBuilder;
    }

    private static async Task<IResult> GetAgenciesAsync(IMediator mediator, CancellationToken cancellationToken)
    {
        var query = new GetAgenciesQuery();
        var result = await mediator.QueryAsync<GetAgenciesQuery, List<GetAgenciesResponse>>(query, cancellationToken);

        return Results.Ok(result);
    }
}
