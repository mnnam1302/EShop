using EShop.Finance.Application.Services.IntegrationProvider.Models;

namespace EShop.Finance.Application.Services.IntegrationProvider.TemplateData;

public sealed class PaymentBookingTemplateModel : TemplateDataModelBase
{
    [TemplateData]
    public required string TenantId { get; init; }

    [TemplateData]
    public required string AccountId { get; init; }

    [TemplateData]
    public required string OrderId { get; init; }

    [TemplateData]
    public required string BuyerId { get; init; }

    [TemplateData]
    public required string PaymentId { get; init; }

    [TemplateData]
    public int Sequence { get; private init; }

    [TemplateData]
    public decimal Amount { get; private init; }

    [TemplateData]
    public string Currency { get; private init; } = string.Empty;

    [TemplateData]
    public DateOnly DueDate { get; private init; }

    public static PaymentBookingTemplateModel Parse(PaymentBookingContext payment, string dateFormat)
    {
        var model = new PaymentBookingTemplateModel
        {
            ShortDateFormat = dateFormat,
            TenantId = payment.TenantId,
            AccountId = payment.AccountId.ToString(),
            OrderId = payment.OrderId.ToString(),
            BuyerId = payment.BuyerId,
            PaymentId = payment.PaymentId.ToString(),
            Sequence = payment.Sequence,
            Amount = payment.Amount,
            Currency = payment.Currency,
            DueDate = payment.DueDate,
        };

        model.BuildTemplateData();
        return model;
    }
}
