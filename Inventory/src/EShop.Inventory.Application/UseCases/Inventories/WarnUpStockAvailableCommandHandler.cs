using EShop.Shared.Contracts.Abstractions.Mediator;

namespace EShop.Inventory.Application.UseCases.Inventories;

public sealed class WarnUpStockAvailableCommand : ICommand
{
    public required List<Guid> VariantIds { get; init; }
}

internal sealed class WarnUpStockAvailableCommandHandler
{
}
