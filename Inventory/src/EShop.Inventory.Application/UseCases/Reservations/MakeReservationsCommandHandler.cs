using EShop.Inventory.Application.Services;
using EShop.Inventory.Domain.Commands;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;

namespace EShop.Inventory.Application.UseCases.Reservations;

internal class MakeReservationsCommandHandler : ICommandHandler<MakeReservationsCommand>
{
    private readonly IStockOrderCacheService _stockOrderCacheService;

    public MakeReservationsCommandHandler(IStockOrderCacheService stockOrderCacheService)
    {
        _stockOrderCacheService = stockOrderCacheService;
    }

    public Task<Result> HandleAsync(MakeReservationsCommand command, CancellationToken cancellationToken)
    {
        // LUA & CAS

        throw new NotImplementedException();
    }
}
