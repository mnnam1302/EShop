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
        group.MapGet("public-key/{tenantId}", GetPublicKeyAsync);
        group.MapGet("public-key/{tenantId}/{keyId}", GetSpecificPublicKeyAsync);
        group.MapGet(".well-known/jwks/{tenantId}", GetJwksAsync);

        return endpoints;
    }

    private static async Task<IResult> LoginAsync([FromBody] LoginQuery query, [FromServices] IMediator mediator)
    {
        var result = await mediator.QueryAsync<LoginQuery, AuthenticationResponse>(query);
        if (result.IsFailure)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetPublicKeyAsync([FromRoute] string tenantId, [FromServices] IMediator mediator)
    {
        var query = new GetPublicKeyQuery(tenantId);
        var result = await mediator.QueryAsync<GetPublicKeyQuery, PublicKeyResponse>(query);
        
        if (result.IsFailure)
        {
            return Results.NotFound(result.Error);
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetSpecificPublicKeyAsync([FromRoute] string tenantId, [FromRoute] string keyId, [FromServices] IMediator mediator)
    {
        var query = new GetPublicKeyQuery(tenantId, keyId);
        var result = await mediator.QueryAsync<GetPublicKeyQuery, PublicKeyResponse>(query);
        
        if (result.IsFailure)
        {
            return Results.NotFound(result.Error);
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> GetJwksAsync([FromRoute] string tenantId, [FromServices] IMediator mediator)
    {
        var query = new GetJwksQuery(tenantId);
        var result = await mediator.QueryAsync<GetJwksQuery, JwksResponse>(query);
        
        if (result.IsFailure)
        {
            return Results.NotFound(result.Error);
        }

        return Results.Ok(result.Value);
    }
}
