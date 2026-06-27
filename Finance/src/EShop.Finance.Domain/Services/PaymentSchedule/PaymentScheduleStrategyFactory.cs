using EShop.Shared.DomainTools.Exceptions;

namespace EShop.Finance.Domain.Services.PaymentSchedule;

/// <summary>
/// Selects the <see cref="IPaymentScheduleStrategy"/> for a payment frequency. Mirrors the
/// "strategy factory selected by name" pattern from the reference Finance service, but kept
/// DI-free: strategies are stateless singletons, so the registry lives in the pure domain.
/// Add a frequency by adding a strategy here — existing strategies are untouched (Open/Closed).
/// </summary>
public static class PaymentScheduleStrategyFactory
{
    private const string ErrorTitle = "PaymentSchedule";

    private static readonly Dictionary<string, IPaymentScheduleStrategy> Strategies =
        new IPaymentScheduleStrategy[]
        {
            new OneOffPaymentScheduleStrategy(),
            new MonthlyPaymentScheduleStrategy(),
            new QuarterlyPaymentScheduleStrategy(),
            new AnnualPaymentScheduleStrategy(),
        }
        .ToDictionary(strategy => strategy.Frequency, StringComparer.Ordinal);

    public static IPaymentScheduleStrategy Resolve(string frequency) =>
        Strategies.TryGetValue(frequency, out var strategy)
            ? strategy
            : throw new DomainException(ErrorTitle, $"Unsupported payment frequency '{frequency}'.");
}
