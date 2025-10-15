using Asp.Versioning;
using EShop.Shared.Contracts.Services.Identity.Users;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static EShop.Shared.Scoping.ResourceAccessControl.PermissionConstants;

namespace EShop.Identity.Presentation.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{api:apiVersion}/[controller]")]
public class UsersController
{
    private readonly ISender _sender;
    private readonly IUserDetailsProvider _userDetailsProvider;

    public UsersController(ISender sender, IUserDetailsProvider userDetailsProvider)
    {
        _sender = sender;
        _userDetailsProvider = userDetailsProvider;
    }

    [HttpPost]
    [RequirePermission(IdentityPermissions.ManageUsersPermissionId)]
    public async Task<IResult> CreateUser([FromBody] Command.CreateUserCommand request)
    {
        var result = await _sender.Send(request);
        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
        }

        return Results.Ok(result);
    }

    [HttpGet("{id}")]
    [RequireOneOfPermissions(
        IdentityPermissions.ViewUsersPermissionId,
        IdentityPermissions.ManageUsersPermissionId)]
    public async Task<IResult> GetUser([FromRoute] string id)
    {
        var request = new Query.GetUserQuery(id);

        var result = await _sender.Send(request);
        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
        }

        return Results.Ok(result);
    }


    [HttpGet("{id}/permissions")]
    [RequireAuthenticatedUser]
    public async Task<IResult> GetCurrentUserPermissions([FromRoute] string id)
    {
        var userId = _userDetailsProvider.AuthenticatedUser.Id ?? id;
        var request = new Query.GetUserPermissionsRequest(userId);

        var result = await _sender.Send(request);
        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
        }

        return Results.Ok(result);
    }
}