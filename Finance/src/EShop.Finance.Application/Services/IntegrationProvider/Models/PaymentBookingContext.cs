namespace EShop.Finance.Application.Services.IntegrationProvider.Models;

public sealed record PaymentBookingContext
{
    public required string TenantId { get; init; }
    public required Guid AccountId { get; init; }
    public required Guid OrderId { get; init; }
    public required string BuyerId { get; init; }
    public required Guid PaymentId { get; init; }
    public required int Sequence { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required DateOnly DueDate { get; init; }
}
