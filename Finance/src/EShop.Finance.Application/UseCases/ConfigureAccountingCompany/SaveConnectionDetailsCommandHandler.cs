using EShop.Finance.Application.Services.IntegrationProvider;
using EShop.Finance.Domain.Aggregates.AccountingCompany.Commands;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;

namespace EShop.Finance.Application.UseCases.ConfigureAccountingCompany;

internal sealed class SaveConnectionDetailsCommandHandler(
    IConnectionDetailsStore connectionDetailsStore) : ICommandHandler<SaveConnectionDetailsCommand>
{
    public async Task<Result> HandleAsync(SaveConnectionDetailsCommand command, CancellationToken cancellationToken)
    {
        await connectionDetailsStore.SaveConnectionDetails(command.TenantId, command.ConnectionDetails, cancellationToken);
        return Result.Success();
    }
}
