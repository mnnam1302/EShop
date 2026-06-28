using EShop.Finance.Domain.Abstractions;
using EShop.Finance.Domain.Aggregates.Account;
using EShop.Finance.Domain.Aggregates.Account.Commands;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Order.Saga;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.Exceptions;
using EShop.Shared.DomainTools.UnitOfWorks;
using Microsoft.Extensions.Logging;

namespace EShop.Finance.Application.UseCases.CreateFinanceAccount;

internal sealed class CreateAccountCommandHandler(
    IAccountRepository accountRepository,
    IUnitOfWork unitOfWork,
    IEventBus eventBus,
    ILogger<CreateAccountCommandHandler> logger) : ICommandHandler<CreateAccountCommand>
{
    public async Task<Result> HandleAsync(CreateAccountCommand command, CancellationToken cancellationToken)
    {
        var existing = await accountRepository.FindByOrderIdAsync(command.OrderId, cancellationToken: cancellationToken);
        if (existing is not null)
        {
            await PublishScheduledAsync(command, existing, cancellationToken);
            return Result.Success();
        }

        try
        {
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
                "Created account {AccountId} for Order {OrderId} with {Count} payment(s) ({Frequency}).",
                account.Id, command.OrderId, account.Payments.Count, account.PaymentFrequency);

            await PublishScheduledAsync(command, account, cancellationToken);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            logger.LogWarning(ex, "Could not schedule payments for Order {OrderId}: {Reason}", command.OrderId, ex.Message);
            await PublishFailedAsync(command, ex.Message, cancellationToken);
            return Result.Failure(new Error("Finance.Schedule.Invalid", ex.Message));
        }
    }

    private async Task PublishScheduledAsync(CreateAccountCommand command, Account account, CancellationToken cancellationToken)
    {
        await eventBus.PublishAsync(new OrderPaymentScheduled
        {
            OrderId = account.OrderId,
            AccountId = account.Id,
            PaymentCount = account.Payments.Count,
            TenantId = command.TenantId,
            ActionUserId = command.ActionUserId,
            ActionUserType = command.ActionUserType,
        }, cancellationToken);
    }

    private async Task PublishFailedAsync(CreateAccountCommand command, string reason, CancellationToken cancellationToken)
    {
        await eventBus.PublishAsync(new OrderPaymentScheduleFailed
        {
            OrderId = command.OrderId,
            Reason = reason,
            TenantId = command.TenantId,
            ActionUserId = command.ActionUserId,
            ActionUserType = command.ActionUserType,
        }, cancellationToken);
    }
}
