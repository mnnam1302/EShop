using EShop.Authorization.Application.UseCases.Roles;
using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Authorization.API.APIs;

public static class RoleEndpointHandler
{
    private const string BaseUrl = "api/v{version:apiVersion}/roles";

    public static IEndpointRouteBuilder MapRoleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .NewVersionedApi("Roles")
            .MapGroup(BaseUrl)
            .HasApiVersion(1)
            .RequireAuthorization()
            .RequireFeatureFilter(FeatureConstants.Authorization.CustomRoles);

        group.MapGet("", GetRolesAsync)
            .RequireOneOfPermissionsFilter(PermissionConstants.Authorization.ManageRoles, PermissionConstants.Authorization.ViewRoles);

        group.MapGet("/{roleId}", GetRoleDetailsAsync)
            .RequireOneOfPermissionsFilter(PermissionConstants.Authorization.ManageRoles, PermissionConstants.Authorization.ViewRoles);

        return endpoints;
    }


    private static async Task<IResult> GetRolesAsync([FromQuery] string? name, [FromServices] IMediator mediator, CancellationToken cancellationToken)
    {
        var query = new GetRolesQuery(name);

        var result = await mediator.QueryAsync<GetRolesQuery, List<RoleResponse>>(query, cancellationToken);

        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> GetRoleDetailsAsync([FromRoute] Guid roleId, [FromServices] IMediator mediator, CancellationToken cancellationToken)
    {
        var query = new GetRoleDetailsQuery(roleId);

        var result = await mediator.QueryAsync<GetRoleDetailsQuery, RoleDetailsResponse>(query, cancellationToken);

        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
        }

        return Results.Ok(result);
    }
}
