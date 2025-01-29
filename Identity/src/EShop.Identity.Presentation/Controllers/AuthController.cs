using Asp.Versioning;
using EShop.Identity.Presentation.Abstractions;
using EShop.Shared.Contracts.Services.Identity.Auth;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping;
using EShop.Shared.Scoping.ResourceAccessControl;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Identity.Presentation.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthenticationController : ApiEndpoint
{
    private readonly ISender _sender;
    private readonly IUserDetailsProvider _userDetailsProvider;

    public AuthenticationController(ISender sender, IUserDetailsProvider userDetailsProvider)
    {
        _sender = sender;
        _userDetailsProvider = userDetailsProvider;
    }

    [HttpPost("register")]
    public async Task<IResult> Register([FromBody] EShop.Shared.Contracts.Services.Identity.Users.Command.RegisterUser command)
    {
        var result = await _sender.Send(command);

        if (result.IsFailure)
        {
            return HandlerFailure(result);
        }

        return Results.Created("", result);
    }

    [HttpPost("login")]
    public async Task<IResult> Login([FromBody] Query.Login query)
    {
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return HandlerFailure(result);
        }

        return Results.Ok(result);
    }

    [HttpGet("logout")]
    [RequireAuthenticatedUser]
    public async Task<IResult> Logout()
    {
        var userId = _userDetailsProvider.AuthenticatedUser.ActionUserId;
        var result = await _sender.Send(new Command.Logout(userId));

        if (result.IsFailure)
        {
            return HandlerFailure(result);
        }

        return Results.Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IResult> RefreshToken([FromBody] Query.Refresh request)
    {
        var query = new Query.Refresh
        {
            AccessToken = JwtEncodedStringHelper.GetJwtEncodedString(_userDetailsProvider.GetRawAccessToken()),
            RefreshToken = request.RefreshToken
        };
        var result = await _sender.Send(query);

        if (result.IsFailure)
        {
            return HandlerFailure(result);
        }

        return Results.Ok(result);
    }
}