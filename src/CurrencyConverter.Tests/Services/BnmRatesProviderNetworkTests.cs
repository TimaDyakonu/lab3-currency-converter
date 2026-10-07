using System.Net.Http;
using CurrencyConverter.Core.Parsing;
using CurrencyConverter.Core.Services;
using CurrencyConverter.Core.Storage;
using CurrencyConverter.Tests.Fixtures;

namespace CurrencyConverter.Tests.Services;

public class BnmRatesProviderNetworkTests
{
    private readonly BnmXmlParser _parser = new();

    [Fact]
    public async Task GetLatestAsync_NoNetworkWithSavedCache_ReturnsCachedRatesWithoutThrowing()
    {
        var tempCachePath = Path.Combine(Path.GetTempPath(), $"cache_test_{Guid.NewGuid():N}.json");
        try
        {
            var cache = new JsonRateCache(tempCachePath);
            var cachedDate = new DateOnly(2026, 10, 5);
            var savedSnapshot = TestSnapshotBuilder.CreateValidSnapshot(cachedDate);
            await cache.SaveAsync(savedSnapshot);

            var handler = new MockHttpMessageHandler();
            handler.SetException(new HttpRequestException("No network connection"));
            var httpClient = new HttpClient(handler);

            var provider = new BnmRatesProvider(httpClient, _parser, cache);
            var requestedDate = new DateOnly(2026, 10, 7);

            var outcome = await provider.GetLatestAsync(requestedDate);

            Assert.True(outcome.IsSuccess);
            Assert.NotNull(outcome.Snapshot);
            Assert.Equal(cachedDate, outcome.Snapshot.RateDate);
            Assert.True(outcome.UsedFallback);
            Assert.Contains(cachedDate.ToString("dd.MM.yyyy"), outcome.StatusMessage);
        }
        finally
        {
            if (File.Exists(tempCachePath)) File.Delete(tempCachePath);
        }
    }

    [Fact]
    public async Task GetLatestAsync_NoNetworkAndNoCache_ReturnsFailureWithoutThrowing()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"missing_cache_{Guid.NewGuid():N}.json");
        var cache = new JsonRateCache(nonExistentPath);

        var handler = new MockHttpMessageHandler();
        handler.SetException(new HttpRequestException("Network down"));
        var httpClient = new HttpClient(handler);

        var provider = new BnmRatesProvider(httpClient, _parser, cache);
        var requestedDate = new DateOnly(2026, 10, 7);

        var outcome = await provider.GetLatestAsync(requestedDate);

        Assert.False(outcome.IsSuccess);
        Assert.Null(outcome.Snapshot);
        Assert.NotNull(outcome.FailureReason);
        Assert.Contains("Network unavailable", outcome.FailureReason);
    }

    [Fact]
    public async Task GetLatestAsync_TimeoutWithSavedCache_ReturnsCachedRates()
    {
        var tempCachePath = Path.Combine(Path.GetTempPath(), $"timeout_cache_{Guid.NewGuid():N}.json");
        try
        {
            var cache = new JsonRateCache(tempCachePath);
            var cachedDate = new DateOnly(2026, 10, 6);
            await cache.SaveAsync(TestSnapshotBuilder.CreateValidSnapshot(cachedDate));

            var handler = new MockHttpMessageHandler();
            handler.SetException(new TaskCanceledException("Request timed out"));
            var httpClient = new HttpClient(handler);

            var provider = new BnmRatesProvider(httpClient, _parser, cache);

            var outcome = await provider.GetLatestAsync(new DateOnly(2026, 10, 7));

            Assert.True(outcome.IsSuccess);
            Assert.NotNull(outcome.Snapshot);
            Assert.Equal(cachedDate, outcome.Snapshot.RateDate);
            Assert.True(outcome.UsedFallback);
        }
        finally
        {
            if (File.Exists(tempCachePath)) File.Delete(tempCachePath);
        }
    }
}
