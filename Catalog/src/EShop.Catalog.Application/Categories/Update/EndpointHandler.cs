using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Catalog.Application.Categories.Update;

public static class EndpointHandler
{
    public static RouteGroupBuilder MapUpdateCategory(this RouteGroupBuilder categoryEndpointBuilder)
    {
        categoryEndpointBuilder.MapPut("/{id}", UpdateCategoryAsync)
            .RequirePermissionFilter(PermissionConstants.Catalog.ManageCategories);

        return categoryEndpointBuilder;
    }

    private static async Task<IResult> UpdateCategoryAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateCategoryRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCategoryCommand
        {
            Id = id,
            Name = request.Name,
            Reference = request.Reference,
            Slug = request.Slug,
            ParentId = request.ParentId
        };

        var result = await mediator.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.NoContent();
    }
}