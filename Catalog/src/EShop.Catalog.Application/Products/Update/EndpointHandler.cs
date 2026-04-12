using EShop.Shared.CQRS;
using EShop.Shared.DomainTools.Exceptions;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Catalog.Application.Products.Update;

public static class EndpointHandler
{
    public static RouteGroupBuilder MapUpdateProduct(this RouteGroupBuilder categoryEndpointBuilder)
    {
        categoryEndpointBuilder.MapPut("/{id}", UpdateProductAsync)
            .RequirePermissionFilter(PermissionConstants.Catalog.ManageProducts);

        return categoryEndpointBuilder;
    }

    private static async Task<IResult> UpdateProductAsync(
        [FromRoute] string id,
        [FromBody] UpdateProductRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var productId))
        {
            throw new BadRequestException($"Invalid product id {id}.");
        }

        var command = new UpdateProductCommand
        {
            Id = productId,
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Tags = request.Tags,
            Slug = request.Slug,
            Images = request.Images,
            Groups = request.Groups
        };

        var result = await mediator.SendAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return ApiEndpointHandler.Failure(result);
        }

        return Results.NoContent();
    }
}