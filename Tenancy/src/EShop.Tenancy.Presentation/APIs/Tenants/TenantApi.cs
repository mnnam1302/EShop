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

namespace EShop.Tenancy.Presentation.APIs.Tenants;

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

        group.MapPost("", CreateTenantV1Async)
            .RequireSystemUserFilter();

        group.MapGet("{tenantId}", GetTenantDetailsAsync)
            .RequireSystemUserFilter();
    }

    private static async Task<IResult> CreateTenantV1Async(
        [FromServices] ISender sender,
        [FromBody] Command.CreateTenantCommand request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(request, cancellationToken);

        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
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
            return ApiResultHandler.HandleFailure(result);
        }

        return Results.Ok(result);
    }
}