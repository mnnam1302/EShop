using System.Globalization;
using EShop.Finance.Application.Services.IntegrationProvider.Authentication;
using EShop.Finance.Application.Services.IntegrationProvider.Configuration;
using EShop.Finance.Application.Services.IntegrationProvider.Models;
using static EShop.Finance.Application.Services.IntegrationProvider.Configuration.FinanceConfigurationConstants;

namespace EShop.Finance.Application.Services.IntegrationProvider.Generic;

/// <summary>
/// Per-booking context for the generic HTTP provider: the parsed <see cref="FinanceConfiguration"/>,
/// the tenant's <see cref="AuthenticationOptions"/>, and helpers to resolve the find/create requests and
/// build the Handlebars template data for a payment.
/// </summary>
internal sealed class HttpAccountingContext(FinanceConfiguration configuration, AuthenticationOptions authOptions)
{
    public FinanceConfiguration Configuration { get; } = configuration;
    public AuthenticationOptions AuthOptions { get; } = authOptions;

    public RequestConfiguration? GetFindPaymentRequest()
        => Configuration.GetRequestConfiguration(Triggers.BookPayment, Actions.Find);

    public RequestConfiguration? GetCreatePaymentRequest()
        => Configuration.GetRequestConfiguration(Triggers.BookPayment, Actions.Create);

    public IReadOnlyDictionary<string, object?> BuildTemplateData(PaymentBookingContext payment)
    {
        var dateFormat = string.IsNullOrWhiteSpace(Configuration.DateFormat) ? "yyyy-MM-dd" : Configuration.DateFormat;

        return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["baseUrl"] = AuthOptions.BaseUrl?.TrimEnd('/'),
            ["tenantId"] = payment.TenantId,
            ["accountId"] = payment.AccountId.ToString(),
            ["orderId"] = payment.OrderId.ToString(),
            ["buyerId"] = payment.BuyerId,
            ["paymentId"] = payment.PaymentId.ToString(),
            ["sequence"] = payment.Sequence.ToString(CultureInfo.InvariantCulture),
            ["amount"] = payment.Amount.ToString(CultureInfo.InvariantCulture),
            ["currency"] = payment.Currency,
            ["dueDate"] = payment.DueDate.ToString(dateFormat, CultureInfo.InvariantCulture),
        };
    }
}
