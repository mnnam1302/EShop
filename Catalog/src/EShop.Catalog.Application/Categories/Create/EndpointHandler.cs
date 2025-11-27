using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Catalog.Application.Categories.Create;

public static class EndpointHandler
{
    public static RouteGroupBuilder MapCreateCategory(this RouteGroupBuilder categoryEndpointBuilder)
    {
        categoryEndpointBuilder.MapPost("/", CreateCategoryAsync);
            //.RequirePermissionFilter(PermissionConstants.Catalog.ManageCategories);

        return categoryEndpointBuilder;
    }

    private static async Task<IResult> CreateCategoryAsync(
        [FromBody] CreateCategoryRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new Command
        {
            Name = request.Name,
            Reference = request.Reference,
            Slug = request.Slug,
            ParentId = request.ParentId
        };

        var result = await mediator.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
        }

        return Results.Created("", result);
    }
}
