using EShop.Authorization.API.Models;
using EShop.Authorization.Application.UseCases.Queries;
using EShop.Shared.CQRS;
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

        group.MapPost("login", LoginAsync);

        return endpoints;
    }

    private static async Task<IResult> LoginAsync([FromBody] LoginRequest request, [FromServices] IMediator mediator)
    {
        var query = new LoginQuery(request.Username, request.Password);

        var result = await mediator.QueryAsync<LoginQuery, AuthenticationResponse>(query);
        if (result.IsFailure)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(result.Value);
    }
}
