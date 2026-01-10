using Carter;
using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Tenancy.Application.UseCases.V1.Commands.Features;
using EShop.Tenancy.Presentation.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace EShop.Tenancy.Presentation.APIs;

public sealed class FeatureApi : ICarterModule
{
    private const string BaseUrl = "api/v{version:apiVersion}/features";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.NewVersionedApi("Features")
            .MapGroup(BaseUrl)
            .HasApiVersion(1)
            .RequireAuthorization()
            .RequireSystemUserFilter();

        group.MapPost("", CreateSystemFeatureAsync);
    }

    private static async Task<IResult> CreateSystemFeatureAsync(
        [FromBody] CreateSystemFeatureRequest request,
        [FromServices] IMediator sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateSystemFeatureCommand
        {
            Id = request.Id,
            Name = request.Name,
            Description = request.Description,
            State = request.State,
            Module = request.Module
        };

        var result = await sender.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.Created();
    }
}