using EShop.Shared.Contracts.Abstractions.Mediator;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;

namespace EShop.Inventory.Application.UseCases.Inventories;

public sealed class DecreaseStockCommand : ICommand
{
    public required Guid VariantId { get; init; }
    public required int Quantity { get; init; }
}

internal sealed class DecreaseStockCommandHandler : ICommandHandler<DecreaseStockCommand>
{
    public Task<Result> HandleAsync(DecreaseStockCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
