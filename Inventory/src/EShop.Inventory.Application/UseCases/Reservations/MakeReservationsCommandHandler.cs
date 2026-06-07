using EShop.Inventory.Application.Services;
using EShop.Inventory.Domain.Abstractions;
using EShop.Inventory.Domain.Commands;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using Microsoft.Extensions.Logging;

namespace EShop.Inventory.Application.UseCases.Reservations;

internal class MakeReservationsCommandHandler(
    IStockOrderCacheService stockOrderCacheService,
    IInventoryRepository repository,
    ILogger<MakeReservationsCommandHandler> logger) : ICommandHandler<MakeReservationsCommand>
{
    // Concurrency & Idempotency
    public async Task<Result> HandleAsync(MakeReservationsCommand command, CancellationToken cancellationToken)
    {
        foreach (var item in command.Items)
        {
            //int oldStockAvailable = await stockOrderCacheService.DecreaseStockCache(item.VariantId, item.Quantity);
            int oldStockAvailable = await stockOrderCacheService.DecreaseStockCacheByLUA(item.VariantId, item.Quantity);

            if (oldStockAvailable == 0)
            {
                logger.LogInformation("StockAvailable < quantity | {OldStock}, {Quantity}", oldStockAvailable, item.Quantity);
                return Result.Failure(new Error("Inventory.Stock", "Inventory is not enough quantity"));
            }

            await repository.DecreaseStockLevel1(item.VariantId, item.Quantity, cancellationToken);
            //await repository.DecreaseStockLevel3CAS(item.VariantId, oldStockAvailable, item.Quantity, cancellationToken);
        }

        return Result.Success();
    }
}
