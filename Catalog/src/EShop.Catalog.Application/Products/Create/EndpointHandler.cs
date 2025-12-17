using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.Abstractions;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Catalog.Application.Products.Create;

public static class EndpointHandler
{
    public static RouteGroupBuilder MapCreateProduct(this RouteGroupBuilder categoryEndpointBuilder)
    {
        categoryEndpointBuilder.MapPost("/", CreateProductAsync)
            .RequirePermissionFilter(PermissionConstants.Catalog.ManageProducts);

        return categoryEndpointBuilder;
    }

    private static async Task<IResult> CreateProductAsync(
        [FromBody] CreateProductRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand
        {
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Price = request.Price,
            DiscountPrice = request.DiscountPrice,
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

        return Results.Created("", result);
    }
}