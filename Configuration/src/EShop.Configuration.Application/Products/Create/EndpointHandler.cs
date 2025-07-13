using EShop.Shared.CQRS;
using EShop.Shared.JsonApi.ResourceAccessControl;
using EShop.Shared.Scoping.ResourceAccessControl;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Configuration.Application.Products.Create;

internal static class EndpointHandler
{
    public static RouteGroupBuilder MapCreateProduct(this RouteGroupBuilder productEndpointBuilder)
    {
        productEndpointBuilder.MapPost("/", CreateProductAsync)
            .RequirePermissionFilter(PermissionConstants.ManageProductsPermissionId)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        return productEndpointBuilder;
    }

    private static async Task<IResult> CreateProductAsync(
        [FromBody] CreateProductRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        var result = await mediator.SendAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return TypedResults.Problem(result.Error.Message, statusCode: StatusCodes.Status400BadRequest);
        }

        return TypedResults.Created();
    }

    private static Command ToCommand(this CreateProductRequest request)
    {
        return new Command
        {
            Name = request.Name,
            AgencyId = request.AgencyId
        };
    }
}
