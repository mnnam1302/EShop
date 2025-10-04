using EShop.Authorization.Application.UseCases.Queries;
using EShop.Shared.CQRS;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Authorization.API.APIs;

public static class AuthenticationEndpointHandler
{
    private static string BaseUrl = "auth/";

    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .NewVersionedApi("Authentication")
            .MapGroup(BaseUrl)
            .HasApiVersion(1);

        group.MapPost("login", LoginV1Async);

        return endpoints;
    }

    private static async Task<IResult> LoginV1Async([FromBody] LoginQuery query, [FromServices] IMediator mediator)
    {
        var result = await mediator.QueryAsync<LoginQuery, AuthenticationResult>(query);
        if (result.IsFailure)
        {
            return Results.BadRequest(new { Errors = result.Error });
        }

        return Results.Ok(result.Value);
    }
}
