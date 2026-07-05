using EShop.Finance.Application.Services.IntegrationProvider.Models;
using EShop.Shared.Contracts.Abstractions.Shared;

namespace EShop.Finance.Application.Services.IntegrationProvider;

public interface IAccountingIntegrationProvider
{
    string Name { get; }

    Task<Result<PaymentBookingResult>> BookPaymentAsync(PaymentBookingContext context, CancellationToken cancellationToken);

    Task<bool> TestConnectionAsync(IReadOnlyDictionary<string, string?> connectionDetails, CancellationToken cancellationToken);
}
