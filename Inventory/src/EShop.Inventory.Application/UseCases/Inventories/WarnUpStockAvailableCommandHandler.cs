using EShop.Inventory.Application.Services;
using EShop.Inventory.Domain.Abstractions;
using EShop.Shared.Contracts.Abstractions.Mediator;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;

namespace EShop.Inventory.Application.UseCases.Inventories;

public sealed class WarnUpStockAvailableCommand : ICommand
{
    public required Guid VariantId { get; init; }
}

internal sealed class WarnUpStockAvailableCommandHandler : ICommandHandler<WarnUpStockAvailableCommand>
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IStockOrderCacheService _stockOrderCacheService;

    public WarnUpStockAvailableCommandHandler(IInventoryRepository inventoryRepository, IStockOrderCacheService stockOrderCacheService)
    {
        _inventoryRepository = inventoryRepository;
        _stockOrderCacheService = stockOrderCacheService;
    }

    public async Task<Result> HandleAsync(WarnUpStockAvailableCommand command, CancellationToken cancellationToken)
    {
        var inventoryDetails = await _inventoryRepository.FindSingleAsync(
            i => i.VariantId == command.VariantId,
            cancellationToken: cancellationToken);

        if (inventoryDetails == null)
        {
            return Result.Failure(new Error("Inventory.NotFound", $"Variant's inventory '{command.VariantId}' is not found."));
        }

        await _stockOrderCacheService.AddStockAvailable(command.VariantId, inventoryDetails.StockAvailable);

        return Result.Success();
    }
}
