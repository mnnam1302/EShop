using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Catalog.Application.Categories.Unpublish;

public static class EndpointHandler
{
    public static RouteGroupBuilder MapUnpublishCategory(this RouteGroupBuilder categoryEndpointBuilder)
    {
        categoryEndpointBuilder.MapPut("/{id}", UnpublishCategoryAsync)
            .RequirePermissionFilter(PermissionConstants.Catalog.ManageCategories);

        return categoryEndpointBuilder;
    }

    private static async Task<IResult> UnpublishCategoryAsync([FromRoute] Guid id, [FromServices] IMediator mediator, CancellationToken cancellationToken)
    {
        var command = new UnpublishCategoryCommand(id);

        var result = await mediator.SendAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return ApiResultHandler.HandleFailure(result);
        }

        return Results.NoContent();
    }
}