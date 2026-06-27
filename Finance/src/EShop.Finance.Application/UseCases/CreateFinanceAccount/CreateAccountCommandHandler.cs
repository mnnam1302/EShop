using EShop.Finance.Domain.Abstractions;
using EShop.Finance.Domain.Aggregates.Account;
using EShop.Finance.Domain.Aggregates.Account.Commands;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.UnitOfWorks;
using Microsoft.Extensions.Logging;

namespace EShop.Finance.Application.UseCases.CreateFinanceAccount;

internal sealed class CreateAccountCommandHandler(
    IAccountRepository accountRepository,
    IUnitOfWork unitOfWork,
    ILogger<CreateAccountCommandHandler> logger) : ICommandHandler<CreateAccountCommand>
{
    public async Task<Result> HandleAsync(CreateAccountCommand command, CancellationToken cancellationToken)
    {
        var existing = await accountRepository.FindByOrderIdAsync(command.OrderId, cancellationToken: cancellationToken);
        if (existing is not null)
        {
            return Result.Success();
        }

        var account = Account.Create(
            command.OrderId,
            command.BuyerId,
            command.TotalAmount,
            command.Currency,
            command.PaymentFrequency,
            command.TenantId);

        account.CalculateScheduledPayments(DateOnly.FromDateTime(DateTime.UtcNow));

        accountRepository.Add(account);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Created an account {AccountId} for Order {OrderId} with {Count} instalment(s) ({Frequency}).",
            account.Id, command.OrderId, account.Payments.Count, account.PaymentFrequency);

        return Result.Success();
    }
}
