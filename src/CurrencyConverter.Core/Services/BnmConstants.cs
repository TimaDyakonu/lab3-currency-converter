using System.Globalization;

namespace CurrencyConverter.Core.Services;

public static class BnmConstants
{
    public const string DateFormat = "dd.MM.yyyy";
    public const string SourceName = "National Bank of Moldova (BNM)";
    public const string BaseCurrencyCode = "MDL";
    public const string BaseCurrencyName = "Moldovan Leu";
    public const string UrlTemplate = "https://www.bnm.md/en/official_exchange_rates?get_xml=1&date={0}";
    public const int MaxFallbackDays = 7;
    public const int DefaultTimeoutSeconds = 10;

    public static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;
}
