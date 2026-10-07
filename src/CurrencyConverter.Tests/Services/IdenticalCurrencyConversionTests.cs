using CurrencyConverter.Core.Models;
using CurrencyConverter.Core.Services;
using CurrencyConverter.Tests.Fixtures;

namespace CurrencyConverter.Tests.Services;

public class IdenticalCurrencyConversionTests
{
    private readonly CurrencyConverter.Core.Services.CurrencyConverter _converter = new();

    [Theory]
    [InlineData("EUR", 100)]
    [InlineData("USD", 45.67)]
    [InlineData("MDL", 1250)]
    [InlineData("RUB", 5000)]
    public void Convert_IdenticalCurrencies_ReturnsOriginalAmountAndPreservesMetadata(string currencyCode, decimal amount)
    {
        var snapshot = TestSnapshotBuilder.CreateValidSnapshot(new DateOnly(2026, 10, 7));
        var request = new ConversionRequest(amount, currencyCode, currencyCode);

        var result = _converter.Convert(request, snapshot, usedFallback: false);

        Assert.True(result.IsSuccess);
        Assert.Equal(amount, result.Amount);
        Assert.Equal(Math.Round(amount, 2, MidpointRounding.AwayFromZero), result.DisplayedAmount);
        Assert.Equal(snapshot.RateDate, result.RateDate);
        Assert.Equal(snapshot.SourceName, result.SourceName);
        Assert.False(result.UsedFallback);
        Assert.Contains("Identical currency", result.StatusMessage);
    }

    [Fact]
    public void Convert_IdenticalCurrencies_WithFallback_PreservesFallbackFlag()
    {
        var snapshot = TestSnapshotBuilder.CreateValidSnapshot(new DateOnly(2026, 10, 5));
        var request = new ConversionRequest(50m, "USD", "USD");

        var result = _converter.Convert(request, snapshot, usedFallback: true);

        Assert.True(result.IsSuccess);
        Assert.Equal(50m, result.Amount);
        Assert.Equal(50.00m, result.DisplayedAmount);
        Assert.Equal(snapshot.RateDate, result.RateDate);
        Assert.True(result.UsedFallback);
    }
}
