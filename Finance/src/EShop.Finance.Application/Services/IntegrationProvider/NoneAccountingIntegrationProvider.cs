using EShop.Finance.Application.Services.IntegrationProvider.Models;
using EShop.Finance.Domain.Aggregates.AccountingCompany;
using EShop.Shared.Contracts.Abstractions.Shared;

namespace EShop.Finance.Application.Services.IntegrationProvider;

public sealed class NoneAccountingIntegrationProvider : IAccountingIntegrationProvider
{
    public string Name => AccountingProviderNames.None;

    public Task<Result<PaymentBookingResult>> BookPaymentAsync(PaymentBookingContext context, CancellationToken cancellationToken)
    {
        throw new NotSupportedException("The 'None' accounting provider cannot book payments; the booking pipeline must skip it.");
    }

    public Task<bool> TestConnectionAsync(IReadOnlyDictionary<string, string?> connectionDetails, CancellationToken cancellationToken)
    {
        return Task.FromResult(false);
    }
}
