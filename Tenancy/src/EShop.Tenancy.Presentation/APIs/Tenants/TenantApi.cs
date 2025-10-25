using Carter;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
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

        group.MapPost("/", CreateTenantV1Async)
            .RequireSupportUserFilter();
    }

    private static async Task<IResult> CreateTenantV1Async(ISender sender, [FromBody] Command.CreateTenantCommand request)
    {
        var result = await sender.Send(request);

        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
        }

        return Results.Created("", result);
    }
}