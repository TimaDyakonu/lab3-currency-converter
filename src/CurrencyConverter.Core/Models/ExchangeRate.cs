using System.Text.Json.Serialization;

namespace CurrencyConverter.Core.Models;

public record ExchangeRate
{
    public string CurrencyCode { get; }
    public string CurrencyName { get; }
    public decimal Nominal { get; }
    public decimal Value { get; }
    public decimal RatePerMdlUnit { get; }

    public ExchangeRate(string currencyCode, string currencyName, decimal nominal, decimal value)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new ArgumentException("Currency code cannot be empty.", nameof(currencyCode));
        if (nominal <= 0m)
            throw new ArgumentOutOfRangeException(nameof(nominal), "Nominal must be greater than zero.");
        if (value <= 0m)
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than zero.");

        CurrencyCode = currencyCode.Trim().ToUpperInvariant();
        CurrencyName = string.IsNullOrWhiteSpace(currencyName) ? CurrencyCode : currencyName.Trim();
        Nominal = nominal;
        Value = value;
        RatePerMdlUnit = value / nominal;
    }

    [JsonConstructor]
    public ExchangeRate(string currencyCode, string currencyName, decimal nominal, decimal value, decimal ratePerMdlUnit)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            throw new ArgumentException("Currency code cannot be empty.", nameof(currencyCode));
        if (nominal <= 0m)
            throw new ArgumentOutOfRangeException(nameof(nominal), "Nominal must be greater than zero.");
        if (value <= 0m)
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than zero.");
        if (ratePerMdlUnit <= 0m)
            throw new ArgumentOutOfRangeException(nameof(ratePerMdlUnit), "RatePerMdlUnit must be greater than zero.");

        CurrencyCode = currencyCode.Trim().ToUpperInvariant();
        CurrencyName = string.IsNullOrWhiteSpace(currencyName) ? CurrencyCode : currencyName.Trim();
        Nominal = nominal;
        Value = value;
        RatePerMdlUnit = ratePerMdlUnit;
    }
}
