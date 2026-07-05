using EShop.Finance.Application.Services.IntegrationProvider;
using EShop.Finance.Application.Services.IntegrationProvider.Models;
using EShop.Finance.Domain.Abstractions;
using EShop.Finance.Domain.Aggregates.Account;
using EShop.Finance.Domain.Aggregates.Account.Commands;
using EShop.Finance.Domain.Aggregates.AccountingCompany;
using EShop.Finance.Domain.Enums;
using EShop.Shared.Contracts.Abstractions.MessageBus;
using EShop.Shared.Contracts.Abstractions.Shared;
using EShop.Shared.Contracts.Services.Finance;
using EShop.Shared.CQRS.Command;
using EShop.Shared.DomainTools.UnitOfWorks;
using Microsoft.Extensions.Logging;

namespace EShop.Finance.Application.UseCases.BookAccountPayments;

internal sealed class BookAccountPaymentsCommandHandler(
    IAccountRepository accounts,
    IAccountingIntegrationProviderFactory providerFactory,
    IUnitOfWork unitOfWork,
    IEventBus eventBus,
    ILogger<BookAccountPaymentsCommandHandler> logger) : ICommandHandler<BookAccountPaymentsCommand>
{
    public async Task<Result> HandleAsync(BookAccountPaymentsCommand command, CancellationToken cancellationToken)
    {
        var account = await accounts.FindByIdAsync(command.AccountId, trackChanges: true, cancellationToken, a => a.Payments);
        if (account is null)
        {
            return Result.Failure(new Error("Finance.Booking.AccountNotFound", $"Account {command.AccountId} not found."));
        }

        var provider = await providerFactory.Create(account.TenantId, cancellationToken);
        if (provider.Name.Equals(AccountingProviderNames.None, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogDebug("Tenant {TenantId} has no accounting provider configured; skipping booking.", account.TenantId);
            return Result.Success();
        }

        var toBook = account.Payments
            .Where(p => p.Status == PaymentStatus.Pending && string.IsNullOrEmpty(p.ExternalBookingReference))
            .OrderBy(p => p.Sequence)
            .ToList();

        var failures = new List<(Guid PaymentId, string Reason)>();

        foreach (var payment in toBook)
        {
            var result = await provider.BookPaymentAsync(ToBookingContext(account, payment), cancellationToken);
            if (result.IsSuccess)
            {
                account.BookPayment(payment.Id, result.Value.ExternalReference);
            }
            else
            {
                logger.LogWarning("Booking payment {PaymentId} of account {AccountId} failed: {Reason}", payment.Id, account.Id, result.Error.Message);
                failures.Add((payment.Id, result.Error.Message));
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var (paymentId, reason) in failures)
        {
            await eventBus.PublishAsync(new PaymentBookingFailed
            {
                AccountId = account.Id,
                PaymentId = paymentId,
                Reason = reason,
                TenantId = account.TenantId,
                ActionUserId = "system",
                ActionUserType = "System",
            }, cancellationToken);
        }

        return failures.Count == 0
            ? Result.Success()
            : Result.Failure(new Error("Finance.Booking.PartialFailure", $"{failures.Count} payment(s) failed to book."));
    }

    private static PaymentBookingContext ToBookingContext(Account account, Payment payment) => new()
    {
        TenantId = account.TenantId,
        AccountId = account.Id,
        OrderId = account.OrderId,
        BuyerId = account.BuyerId,
        PaymentId = payment.Id,
        Sequence = payment.Sequence,
        Amount = payment.Amount,
        Currency = payment.Currency,
        DueDate = payment.DueDate,
    };
}
