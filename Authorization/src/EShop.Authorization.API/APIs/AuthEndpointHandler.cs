using EShop.Authorization.API.Models;
using EShop.Authorization.Application.UseCases.Authentication;
using EShop.Shared.Authentication.Abstractions;
using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Authorization.API.APIs;

public static class AuthEndpointHandler
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .NewVersionedApi("Auth")
            .MapGroup("api/v{version:apiVersion}/auth")
            .HasApiVersion(1)
            .RequireAuthorization();

        group.MapPost("login", LoginAsync).AllowAnonymous();
        group.MapPost("refreshToken", RefreshTokenAsync).AllowAnonymous();
        group.MapPost("logout", LogoutAsync);

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        [FromBody] LoginRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new LoginQuery(request.Username, request.Password);

        var result = await mediator.QueryAsync<LoginQuery, AuthenticationResponse>(query, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> LogoutAsync(
        [FromBody] LogoutRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new LogoutCommand(request.UserId);

        var result = await mediator.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.Ok();
    }

    private static async Task<IResult> RefreshTokenAsync(
        [FromBody] RefreshTokenRequest request,
        [FromServices] IUserDetailsProvider userDetailsProvider,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new RefreshTokenQuery(userDetailsProvider.GetRawAccessToken(), request.RefreshToken);

        var result = await mediator.QueryAsync<RefreshTokenQuery, AuthenticationResponse>(query, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.Ok(result);
    }
}
