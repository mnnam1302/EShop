using Carter;
using EShop.Shared.Contracts.Services.Tenancy.Tenants;
using EShop.Shared.JsonApi.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace EShop.Tenancy.Presentation.APIs.Tenants;

public class TenantApi : ApiEndpointBase, ICarterModule
{
    private const string BaseUrl = "api/v{version:apiVersion}/tenants";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group1 = app
            .NewVersionedApi("Tenants")
            .MapGroup(BaseUrl)
            .HasApiVersion(1);

        group1.MapPost("/", CreateTenantV1Async);
    }

    private static async Task<IResult> CreateTenantV1Async(ISender sender, [FromBody] Command.CreateTenantCommand request)
    {
        var result = await sender.Send(request);

        if (result.IsFailure)
        {
            return HandlerFailure(result);
        }

        return Results.Created("", result);
    }
}