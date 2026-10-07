using CurrencyConverter.Core.Models;
using CurrencyConverter.Core.Services;
using CurrencyConverter.Tests.Fixtures;

namespace CurrencyConverter.Tests.Services;

public class CurrencyConverterTests
{
    private readonly CurrencyConverter.Core.Services.CurrencyConverter _converter = new();
    private readonly RateSnapshot _snapshot = TestSnapshotBuilder.CreateValidSnapshot();

    [Fact]
    public void Convert_DirectFromForeignToMdl_CalculatesCorrectly()
    {
        // 100 EUR to MDL, EUR = 19.50 MDL
        var request = new ConversionRequest(100m, "EUR", "MDL");

        var result = _converter.Convert(request, _snapshot);

        Assert.True(result.IsSuccess);
        Assert.Equal(1950.00m, result.Amount);
        Assert.Equal(1950.00m, result.DisplayedAmount);
        Assert.Equal(_snapshot.RateDate, result.RateDate);
        Assert.Equal(_snapshot.SourceName, result.SourceName);
    }

    [Fact]
    public void Convert_ReverseFromMdlToForeign_CalculatesCorrectly()
    {
        // 1950 MDL to EUR (EUR = 19.50 MDL) -> 100 EUR
        var request = new ConversionRequest(1950m, "MDL", "EUR");

        var result = _converter.Convert(request, _snapshot);

        Assert.True(result.IsSuccess);
        Assert.Equal(100.00m, result.DisplayedAmount);
    }

    [Fact]
    public void Convert_ForeignToForeignThroughMdl_CalculatesCorrectly()
    {
        // EUR (19.50) to USD (17.80): 100 EUR = 1950 MDL. 1950 / 17.80 = 109.55056...
        var request = new ConversionRequest(100m, "EUR", "USD");

        var result = _converter.Convert(request, _snapshot);

        Assert.True(result.IsSuccess);
        var expectedAmount = 100m * 19.50m / 17.80m;
        Assert.Equal(expectedAmount, result.Amount);
        Assert.Equal(109.55m, result.DisplayedAmount);
    }

    [Fact]
    public void Convert_WithNonUnitNominalCurrency_CalculatesCorrectly()
    {
        // RUB has nominal 100, value 19.20 => ratePerUnit = 0.1920
        // Convert 1000 RUB to MDL => 1000 * 0.1920 = 192.00 MDL
        var request = new ConversionRequest(1000m, "RUB", "MDL");

        var result = _converter.Convert(request, _snapshot);

        Assert.True(result.IsSuccess);
        Assert.Equal(192.00m, result.Amount);
        Assert.Equal(192.00m, result.DisplayedAmount);
    }

    [Fact]
    public void Convert_IdenticalCurrencies_ReturnsOriginalAmountWithoutCalculationError()
    {
        var request = new ConversionRequest(123.456m, "EUR", "EUR");

        var result = _converter.Convert(request, _snapshot);

        Assert.True(result.IsSuccess);
        Assert.Equal(123.456m, result.Amount);
        Assert.Equal(123.46m, result.DisplayedAmount);
        Assert.Equal(_snapshot.RateDate, result.RateDate);
    }

    [Fact]
    public void Convert_DisplayedAmount_RoundsToTwoDecimalPlaces()
    {
        // 10 MDL to USD (17.80): 10 / 17.80 = 0.561797... -> 0.56
        var request = new ConversionRequest(10m, "MDL", "USD");

        var result = _converter.Convert(request, _snapshot);

        Assert.True(result.IsSuccess);
        Assert.Equal(0.56m, result.DisplayedAmount);
    }

    [Fact]
    public void Convert_UnknownCurrency_ReturnsCalculationFailure()
    {
        var request = new ConversionRequest(100m, "XYZ", "MDL");

        var result = _converter.Convert(request, _snapshot);

        Assert.False(result.IsSuccess);
        Assert.Contains("XYZ", result.ErrorMessage);
    }
}
