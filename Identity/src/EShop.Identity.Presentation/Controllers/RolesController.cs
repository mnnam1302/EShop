using Asp.Versioning;
using EShop.Shared.Contracts.Abstractions.Paging;
using EShop.Shared.Contracts.Services.Identity.Roles;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Identity.Presentation.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class RolesController : ApiEndpointBase
{
    private readonly ISender _sender;

    public RolesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [RequireOneOfPermissions(
        PermissionConstants.ViewRolesPermissionId,
        PermissionConstants.ManageRolesPermissionId)]
    public async Task<IResult> GetRoles(
        string? name = null,
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new Query.GetRoles(name, Paging.Create(pageIndex, pageSize));
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return HandlerFailure(result);
        }

        return Results.Ok(result);
    }

    [HttpGet("{id}")]
    [RequireOneOfPermissions(Permissions = new string[]
    {
        PermissionConstants.ViewRolesPermissionId,
        PermissionConstants.ManageRolesPermissionId
    })]
    public async Task<IResult> GetRole(string id, CancellationToken cancellationToken)
    {
        var query = new Query.GetRoleById(id);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return HandlerFailure(result);
        }

        return Results.Ok(result);
    }

    [HttpPost]
    [RequirePermission(Permission = PermissionConstants.ManageRolesPermissionId)]
    public async Task<IResult> CreateRole([FromBody] Command.CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request, cancellationToken);

        if (result.IsFailure)
        {
            return HandlerFailure(result);
        }

        return Results.Created("", result);
    }

    [HttpPatch("{id}")]
    [RequirePermission(Permission = PermissionConstants.ManageRolesPermissionId)]
    public async Task<IResult> UpdateRole(string id, [FromBody] Command.UpdateRole request, CancellationToken cancellationToken)
    {
        var command = request with { Id = id };
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandlerFailure(result);
        }

        return Results.Ok(result);
    }

    [HttpDelete("{id}")]
    [RequirePermission(Permission = PermissionConstants.ManageRolesPermissionId)]
    public async Task<IResult> DeleteRole(string id, CancellationToken cancellationToken)
    {
        var command = new Command.DeleteRole(id);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return HandlerFailure(result);
        }

        return Results.NoContent();
    }
}