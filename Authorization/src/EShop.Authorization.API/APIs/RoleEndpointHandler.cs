using EShop.Authorization.API.Models;
using EShop.Authorization.Application.UseCases.Roles;
using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;
using AuhthorizationPermissions = EShop.Shared.Scoping.ResourceAccessControl.PermissionConstants.Authorization;

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

        group.MapPost("", CreateRoleAsync)
            .RequirePermissionFilter(AuhthorizationPermissions.ManageRoles);

        group.MapGet("", GetRolesAsync)
            .RequireOneOfPermissionsFilter(AuhthorizationPermissions.ManageRoles, AuhthorizationPermissions.ViewRoles);

        group.MapGet("/{roleId}", GetRoleDetailsAsync)
            .RequireOneOfPermissionsFilter(AuhthorizationPermissions.ManageRoles, AuhthorizationPermissions.ViewRoles);

        return endpoints;
    }

    private static async Task<IResult> CreateRoleAsync([FromBody] CreateRoleRequest request, [FromServices] IMediator mediator, CancellationToken cancellationToken)
    {
        var command = new CreateRoleCommand
        {
            Name = request.Name,
            Description = request.Description,
            PermissionIds = request.PermissionIds
        };

        var result = await mediator.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
        }

        return Results.Created("", result);
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
