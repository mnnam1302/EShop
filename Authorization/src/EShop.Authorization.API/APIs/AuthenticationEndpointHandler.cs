using EShop.Authorization.API.Models;
using EShop.Authorization.Application.UseCases.Commands;
using EShop.Authorization.Application.UseCases.Queries;
using EShop.Shared.CQRS;
using EShop.Shared.Scoping;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Authorization.API.APIs;

public static class AuthenticationEndpointHandler
{
    private const string BaseUrl = "api/v{version:apiVersion}/auth";

    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .NewVersionedApi("Authentication")
            .MapGroup(BaseUrl)
            .HasApiVersion(1);

        group.MapPost("login", LoginAsync)
            .AllowAnonymous();

        group.MapPost("logout", LogoutAsync)
            .RequireAuthorization();

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
            return Results.Unauthorized();
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> LogoutAsync(
        [FromBody] LogoutRequest request,
        [FromServices] IUserDetailsProvider userDetailsProvider,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new LogoutCommand
        {
            UserId = request.UserId,
            AccessToken = userDetailsProvider.GetRawAccessToken()
        };

        var result = await mediator.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"Logout failed: {result.Error}");
        }

        return Results.Ok();
    }
}
