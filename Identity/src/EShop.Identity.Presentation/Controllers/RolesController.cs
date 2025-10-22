using Asp.Versioning;
using EShop.Shared.Contracts.Abstractions.Pagination;
using EShop.Shared.Contracts.Services.Identity.Roles;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static EShop.Shared.Scoping.ResourceAccessControl.PermissionConstants;

namespace EShop.Identity.Presentation.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class RolesController
{
    private readonly ISender _sender;

    public RolesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [RequireOneOfPermissions(
        IdentityPermissions.ViewRolesPermissionId,
        IdentityPermissions.ManageRolesPermissionId)]
    public async Task<IResult> GetRoles(
        string? name = null,
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new Query.GetRoles(name, PaginationRequest.Create(pageIndex, pageSize));
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
        }

        return Results.Ok(result);
    }

    [HttpGet("{id}")]
    [RequireOneOfPermissions(
        IdentityPermissions.ViewRolesPermissionId,
        IdentityPermissions.ManageRolesPermissionId)]
    public async Task<IResult> GetRole(Guid id, CancellationToken cancellationToken)
    {
        var query = new Query.GetRoleById(id);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
        }

        return Results.Ok(result);
    }

    [HttpPost]
    [RequirePermission(IdentityPermissions.ManageRolesPermissionId)]
    public async Task<IResult> CreateRole([FromBody] Command.CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request, cancellationToken);

        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
        }

        return Results.Created("", result);
    }

    [HttpPatch("{id}")]
    [RequirePermission(IdentityPermissions.ManageRolesPermissionId)]
    public async Task<IResult> UpdateRole(Guid id, [FromBody] Command.UpdateRole request, CancellationToken cancellationToken)
    {
        var command = request with { Id = id };
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
        }

        return Results.Ok(result);
    }

    [HttpDelete("{id}")]
    [RequirePermission(IdentityPermissions.ManageRolesPermissionId)]
    public async Task<IResult> DeleteRole(Guid id, CancellationToken cancellationToken)
    {
        var command = new Command.DeleteRole(id);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
        }

        return Results.NoContent();
    }
}