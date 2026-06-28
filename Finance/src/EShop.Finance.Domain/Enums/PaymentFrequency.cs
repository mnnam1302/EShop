namespace EShop.Finance.Domain.Enums;

/// <summary>
/// How an order total is split into instalments. Mirrors the frequency vocabulary used across
/// the platform: pay everything up front (<see cref="OneOff"/>) or spread over a one-year term.
/// </summary>
public static class PaymentFrequency
{
    public const string OneOff = "OneOff";
    public const string Monthly = "Monthly";
    public const string Quarterly = "Quarterly";
    public const string Annually = "Annually";

    public static readonly string[] All = [OneOff, Monthly, Quarterly, Annually];

    public static bool IsSupported(string? frequency) =>
        frequency is not null && All.Contains(frequency);
}
