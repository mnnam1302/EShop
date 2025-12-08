using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Catalog.Application.Categories.Publish;

public static class EndpointHandler
{
    public static RouteGroupBuilder MapPublishCategory(this RouteGroupBuilder categoryEndpointBuilder)
    {
        categoryEndpointBuilder.MapPut("/{categoryId}", PublishCategoryAsync)
            .RequirePermissionFilter(PermissionConstants.Catalog.ManageCategories);

        return categoryEndpointBuilder;
    }

    private static async Task PublishCategoryAsync([FromRoute] Guid categoryId, [FromServices] IMediator mediator, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
