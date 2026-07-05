using System.Globalization;
using EShop.Finance.Application.Services.IntegrationProvider.Models;

namespace EShop.Finance.Application.Services.IntegrationProvider.Http;

public sealed class PaymentTemplateDataAdapter : ITemplateDataAdapter<PaymentBookingContext>
{
    public IReadOnlyDictionary<string, object?> ToTemplateData(PaymentBookingContext payment, TemplateRenderContext context)
    {
        var dateFormat = string.IsNullOrWhiteSpace(context.DateFormat) ? "yyyy-MM-dd" : context.DateFormat;

        return new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["baseUrl"] = context.BaseUrl?.TrimEnd('/'),
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
