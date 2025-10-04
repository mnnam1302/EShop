using EShop.Authorization.Application.UseCases.Queries;
using EShop.Shared.CQRS;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Authorization.API.APIs;

public static class AuthenticationEndpointHandler
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .NewVersionedApi("Authentication")
            .MapGroup("auth/")
            .HasApiVersion(1);

        group.MapPost("login", LoginV1Async);

        return endpoints;
    }

    private static async Task<IResult> LoginV1Async([FromBody] LoginQuery query, [FromServices] IMediator mediator)
    {
        var result = await mediator.QueryAsync<LoginQuery, AuthenticationResponse>(query);
        if (result.IsFailure)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(result.Value);
    }
}
