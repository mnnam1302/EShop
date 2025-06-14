using System.Collections.Immutable;

namespace EShop.Tenancy.Domain;

public static class SupportedDateTimeFormats
{
    public const string DefaultDateFormat = "dd/MM/yyyy";
    public const string DefaultTimeFormat = "HH:mm";
}

public static class SupportedCurrencies
{
    public const string DefaultCurrencyCode = "NOK";
    public const string DefaultCurrencyDisplayFormat = "ISO";
}

public static class SupportedLanguages
{
    public const string DefaultLanguageCode = "en-gb";

    public static readonly ImmutableList<string> LanguageCodes = ImmutableList.Create
    (
        "en-gb",
        "nb-no",
        "sv-se"
    );
}