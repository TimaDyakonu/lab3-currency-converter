using CurrencyConverter.Core.Models;
using CurrencyConverter.Core.Services;

namespace CurrencyConverter.Tests.Fixtures;

public static class TestSnapshotBuilder
{
    public static RateSnapshot CreateValidSnapshot(
        DateOnly? date = null,
        string sourceName = BnmConstants.SourceName,
        DateTimeOffset? retrievedAt = null)
    {
        var rateDate = date ?? new DateOnly(2026, 10, 7);
        var rates = new List<ExchangeRate>
        {
            new("MDL", "Moldovan Leu", 1m, 1m),
            new("EUR", "Euro", 1m, 19.50m),
            new("USD", "US Dollar", 1m, 17.80m),
            new("RON", "Romanian Leu", 1m, 3.92m),
            new("RUB", "Russian Ruble", 100m, 19.20m), // ratePerUnit = 0.1920
            new("JPY", "Japanese Yen", 100m, 12.30m)  // ratePerUnit = 0.1230
        };

        return new RateSnapshot(rateDate, sourceName, rates, retrievedAt);
    }

    public static string CreateValidXml(DateOnly? date = null)
    {
        var dateStr = (date ?? new DateOnly(2026, 10, 7)).ToString(BnmConstants.DateFormat);
        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<ValCurs Date=""{dateStr}"" name=""Official exchange rates"">
    <Valute ID=""47"">
        <NumCode>978</NumCode>
        <CharCode>EUR</CharCode>
        <Nominal>1</Nominal>
        <Name>Euro</Name>
        <Value>19.5000</Value>
    </Valute>
    <Valute ID=""44"">
        <NumCode>840</NumCode>
        <CharCode>USD</CharCode>
        <Nominal>1</Nominal>
        <Name>US Dollar</Name>
        <Value>17.8000</Value>
    </Valute>
    <Valute ID=""36"">
        <NumCode>643</NumCode>
        <CharCode>RUB</CharCode>
        <Nominal>100</Nominal>
        <Name>Russian Ruble</Name>
        <Value>19.2000</Value>
    </Valute>
</ValCurs>";
    }
}
