using Carter;
using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Tenancy.Application.UseCases.V1.Commands.Tenants;
using EShop.Tenancy.Application.UseCases.V1.Queries.Tenants;
using EShop.Tenancy.Domain.Commands;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace EShop.Tenancy.Presentation.APIs;

public sealed class TenantApi : ICarterModule
{
    private const string BaseUrl = "api/v{version:apiVersion}/tenants";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app
            .NewVersionedApi("Tenants")
            .MapGroup(BaseUrl)
            .HasApiVersion(1)
            .RequireAuthorization();

        group.MapPost("", CreateTenantAsync)
            .RequireSupportUserFilter();

        group.MapGet("{tenantId}", GetTenantDetailsAsync)
            .RequireSystemUserFilter();

        group.MapPatch("{tenantId}/features/{featureId}/enable", EnableTenantFeatureAsync)
            .RequireSystemUserFilter();
    }

    private static async Task<IResult> CreateTenantAsync(
        [FromServices] IMediator mediator,
        [FromBody] CreateTenantCommand request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.Created("", result);
    }

    private static async Task<IResult> GetTenantDetailsAsync(
        [FromRoute] string tenantId,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetTenantDetailsQuery(tenantId);

        var result = await mediator.QueryAsync<GetTenantDetailsQuery, TenantDetailsResponse>(query, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> EnableTenantFeatureAsync(
        [FromRoute] string tenantId,
        [FromRoute] string featureId,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new EnableTenantFeatureCommand
        {
            TenantId = tenantId,
            FeatureId = featureId
        };

        var result = await mediator.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.NoContent();
    }
}
