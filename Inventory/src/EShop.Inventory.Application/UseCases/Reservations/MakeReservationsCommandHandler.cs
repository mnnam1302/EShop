using EShop.Inventory.Domain.Commands;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;

namespace EShop.Inventory.Application.UseCases.Reservations;

internal class MakeReservationsCommandHandler : ICommandHandler<MakeReservationsCommand>
{
    public Task<Result> HandleAsync(MakeReservationsCommand command, CancellationToken cancellationToken)
    {
        // LUA & CAS

        throw new NotImplementedException();
    }
}
