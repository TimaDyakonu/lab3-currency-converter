using System.Text.Json.Serialization;

namespace CurrencyConverter.Core.Models;

public class RateSnapshot
{
    public DateOnly RateDate { get; }
    public string SourceName { get; }
    public IReadOnlyDictionary<string, ExchangeRate> Rates { get; }
    public DateTimeOffset RetrievedAt { get; }

    [JsonConstructor]
    public RateSnapshot(
        DateOnly rateDate,
        string sourceName,
        IReadOnlyDictionary<string, ExchangeRate> rates,
        DateTimeOffset? retrievedAt = null)
    {
        RateDate = rateDate;
        SourceName = string.IsNullOrWhiteSpace(sourceName) ? "National Bank of Moldova (BNM)" : sourceName.Trim();
        RetrievedAt = retrievedAt ?? DateTimeOffset.UtcNow;

        var rateDict = new Dictionary<string, ExchangeRate>(StringComparer.OrdinalIgnoreCase);
        if (rates != null)
        {
            foreach (var kvp in rates)
            {
                rateDict[kvp.Key] = kvp.Value;
            }
        }

        // Must include MDL at rate 1
        if (!rateDict.ContainsKey("MDL"))
        {
            rateDict["MDL"] = new ExchangeRate("MDL", "Moldovan Leu", 1m, 1m, 1m);
        }

        Rates = rateDict;
    }

    public RateSnapshot(
        DateOnly rateDate,
        string sourceName,
        IEnumerable<ExchangeRate> rates,
        DateTimeOffset? retrievedAt = null)
    {
        RateDate = rateDate;
        SourceName = string.IsNullOrWhiteSpace(sourceName) ? "National Bank of Moldova (BNM)" : sourceName.Trim();
        RetrievedAt = retrievedAt ?? DateTimeOffset.UtcNow;

        var rateDict = new Dictionary<string, ExchangeRate>(StringComparer.OrdinalIgnoreCase);
        if (rates != null)
        {
            foreach (var rate in rates)
            {
                if (rateDict.ContainsKey(rate.CurrencyCode))
                {
                    throw new ArgumentException($"Duplicate currency code found: {rate.CurrencyCode}", nameof(rates));
                }
                rateDict[rate.CurrencyCode] = rate;
            }
        }

        // Must include MDL at rate 1
        if (!rateDict.ContainsKey("MDL"))
        {
            rateDict["MDL"] = new ExchangeRate("MDL", "Moldovan Leu", 1m, 1m, 1m);
        }

        Rates = rateDict;
    }

    public ExchangeRate? GetRate(string currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode)) return null;
        return Rates.TryGetValue(currencyCode.Trim(), out var rate) ? rate : null;
    }

    public bool HasCurrency(string currencyCode) =>
        !string.IsNullOrWhiteSpace(currencyCode) && Rates.ContainsKey(currencyCode.Trim());
}
