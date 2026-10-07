using System.Net;
using System.Net.Http;
using CurrencyConverter.Core.Parsing;
using CurrencyConverter.Core.Services;
using CurrencyConverter.Core.Storage;
using CurrencyConverter.Tests.Fixtures;

namespace CurrencyConverter.Tests.Services;

public class BnmRatesProviderFallbackTests
{
    private readonly BnmXmlParser _parser = new();

    [Fact]
    public async Task GetLatestAsync_WeekendOrHolidayEmpty_FallsBackToLatestPrecedingDate()
    {
        var tempCachePath = Path.Combine(Path.GetTempPath(), $"fallback_cache_{Guid.NewGuid():N}.json");
        try
        {
            var cache = new JsonRateCache(tempCachePath);
            var handler = new MockHttpMessageHandler();

            var sunday = new DateOnly(2026, 10, 11);
            var saturday = new DateOnly(2026, 10, 10);
            var friday = new DateOnly(2026, 10, 9);

            // Sunday returns 404
            handler.RegisterResponse(sunday.ToString("dd.MM.yyyy"), HttpStatusCode.NotFound, "");
            // Saturday returns empty string
            handler.RegisterResponse(saturday.ToString("dd.MM.yyyy"), HttpStatusCode.OK, "");
            // Friday returns valid rates
            var fridayXml = TestSnapshotBuilder.CreateValidXml(friday);
            handler.RegisterResponse(friday.ToString("dd.MM.yyyy"), HttpStatusCode.OK, fridayXml);

            var httpClient = new HttpClient(handler);
            var provider = new BnmRatesProvider(httpClient, _parser, cache);

            var outcome = await provider.GetLatestAsync(sunday);

            Assert.True(outcome.IsSuccess);
            Assert.NotNull(outcome.Snapshot);
            Assert.Equal(friday, outcome.Snapshot.RateDate);
            Assert.True(outcome.UsedFallback);
            Assert.Contains(friday.ToString("dd.MM.yyyy"), outcome.StatusMessage);

            // Verify it was cached
            var cacheLoad = await cache.LoadAsync();
            Assert.True(cacheLoad.IsSuccess);
            Assert.Equal(friday, cacheLoad.Snapshot!.RateDate);
        }
        finally
        {
            if (File.Exists(tempCachePath)) File.Delete(tempCachePath);
        }
    }

    [Fact]
    public async Task GetLatestAsync_StopsSearchAfterSevenPrecedingDays()
    {
        var tempCachePath = Path.Combine(Path.GetTempPath(), $"fallback_7days_{Guid.NewGuid():N}.json");
        try
        {
            var cache = new JsonRateCache(tempCachePath);
            var handler = new MockHttpMessageHandler();
            var httpClient = new HttpClient(handler);
            var provider = new BnmRatesProvider(httpClient, _parser, cache);

            var requestedDate = new DateOnly(2026, 10, 10);

            var outcome = await provider.GetLatestAsync(requestedDate);

            // Offset 0 to 7 = 8 requests total
            Assert.Equal(8, handler.RequestedUris.Count);
            Assert.False(outcome.IsSuccess);
            Assert.Contains("no rates published for past 7 days", outcome.FailureReason);
        }
        finally
        {
            if (File.Exists(tempCachePath)) File.Delete(tempCachePath);
        }
    }
}
