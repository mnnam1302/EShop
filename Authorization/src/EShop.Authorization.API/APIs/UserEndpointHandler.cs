using EShop.Authorization.Application.UseCases.Queries;
using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl.Providers.UserOrganizationContextProvider;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Authorization.API.APIs;

public static class UserEndpointHandler
{
    private const string BaseUrl = "api/v{version:apiVersion}/users";

    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .NewVersionedApi("Users")
            .MapGroup(BaseUrl)
            .HasApiVersion(1);

        group.MapGet("/{userId}/permissions", GetUserPermissions).RequireAuthenticatedUser();
        group.MapGet("/{userId}/organizationContext", GetUserOrganizationContext).RequireAuthenticatedUser();

        return endpoints;
    }

    private static async Task<IResult> GetUserPermissions(
        [FromRoute] string userId,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetUserPermissionsQuery(userId);

        var result = await mediator.QueryAsync<GetUserPermissionsQuery, IEnumerable<string>>(query, cancellationToken);

        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> GetUserOrganizationContext(
        [FromRoute] string userId,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetUserOrganizationContextQuery(userId);

        var result = await mediator.QueryAsync<GetUserOrganizationContextQuery, UserOrganizationContext>(query, cancellationToken);

        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
        }

        return Results.Ok(result);
    }
}
