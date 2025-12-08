using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Catalog.Application.Categories.Publish;

public static class EndpointHandler
{
    public static RouteGroupBuilder MapPublishCategory(this RouteGroupBuilder categoryEndpointBuilder)
    {
        categoryEndpointBuilder.MapPut("/{id}", PublishCategoryAsync)
            .RequirePermissionFilter(PermissionConstants.Catalog.ManageCategories);

        return categoryEndpointBuilder;
    }

    private static async Task<IResult> PublishCategoryAsync([FromRoute] Guid id, [FromServices] IMediator mediator, CancellationToken cancellationToken)
    {
        var command = new PublishCategoryCommand(id);

        var result = await mediator.SendAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.NoContent();
    }
}