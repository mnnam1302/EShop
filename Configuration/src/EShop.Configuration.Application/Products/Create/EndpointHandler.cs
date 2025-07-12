using EShop.Shared.CQRS.Command;
using Microsoft.AspNetCore.Mvc;

namespace EShop.Configuration.Application.Products.Create;

internal static class EndpointHandler
{
    public static RouteGroupBuilder MapCreateProduct(this RouteGroupBuilder productEndpointBuilder)
    {
        productEndpointBuilder.MapPost("/", CreateProductAsync)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        return productEndpointBuilder;
    }

    private static async Task<IResult> CreateProductAsync(
        [FromBody] CreateProductRequest request,
        [FromServices] ICommandHandler<Command> commandHandler,
        CancellationToken cancellationToken)
    {
        var command = request.ToCommand();
        var result = await commandHandler.HandleAsync(command, cancellationToken);

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
