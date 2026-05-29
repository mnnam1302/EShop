using EShop.Shared.CQRS.Command;

namespace EShop.Inventory.Application.UseCases.Inventory;

public sealed class WarnUpStockAvailableCommand : ICommand
{
    public required List<Guid> VariantIds { get; init; }
}

internal sealed class WarnUpStockAvailableCommandHandler
{
}
