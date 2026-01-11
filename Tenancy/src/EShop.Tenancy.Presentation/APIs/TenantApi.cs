using Carter;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Tenancy.Application.UseCases.V1.Queries.Tenants;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using static EShop.Shared.Contracts.Services.Tenancy.Features.Query;

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

        group.MapGet("{tenantId}/features", GetTenantFeaturesAsync)
            .RequireSystemUserFilter();
    }

    private static async Task<IResult> CreateTenantAsync(
        [FromServices] ISender sender,
        [FromBody] Command.CreateTenantCommand request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(request, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.Created("", result);
    }

    private static async Task<IResult> GetTenantDetailsAsync(
        [FromRoute] string tenantId,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetTenantDetailsQuery(tenantId);

        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> GetTenantFeaturesAsync(
        [FromRoute] string tenantId,
        [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTenantFeaturesQuery(tenantId), cancellationToken);

        if (result.IsFailure)
        {
            ApiEndpointHandler.Failure(result);

        }

        return Results.Ok(result);
    }
}