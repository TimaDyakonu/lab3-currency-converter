using CurrencyConverter.Core.Storage;
using CurrencyConverter.Tests.Fixtures;

namespace CurrencyConverter.Tests.Storage;

public class JsonRateCachePersistenceTests
{
    [Fact]
    public async Task Cache_PersistsAcrossSeparateInstances_SimulatingAppRestart()
    {
        var tempCachePath = Path.Combine(Path.GetTempPath(), $"persistence_test_{Guid.NewGuid():N}.json");
        try
        {
            var date = new DateOnly(2026, 10, 7);
            var originalSnapshot = TestSnapshotBuilder.CreateValidSnapshot(date);

            // First run: save snapshot through instance 1
            var cacheInstance1 = new JsonRateCache(tempCachePath);
            var saveResult = await cacheInstance1.SaveAsync(originalSnapshot);
            Assert.True(saveResult.IsSuccess);
            Assert.True(File.Exists(tempCachePath));

            // Second run (simulating application restart): create separate instance pointing to same path
            var cacheInstance2 = new JsonRateCache(tempCachePath);
            var loadResult = await cacheInstance2.LoadAsync();

            Assert.True(loadResult.IsSuccess);
            Assert.NotNull(loadResult.Snapshot);
            Assert.Equal(date, loadResult.Snapshot.RateDate);
            Assert.Equal(originalSnapshot.SourceName, loadResult.Snapshot.SourceName);

            // Verify rates are restored properly
            Assert.True(loadResult.Snapshot.HasCurrency("EUR"));
            Assert.True(loadResult.Snapshot.HasCurrency("USD"));
            Assert.True(loadResult.Snapshot.HasCurrency("MDL"));
            Assert.True(loadResult.Snapshot.HasCurrency("RUB"));

            var rub = loadResult.Snapshot.GetRate("RUB");
            Assert.NotNull(rub);
            Assert.Equal(100m, rub.Nominal);
            Assert.Equal(19.20m, rub.Value);
            Assert.Equal(0.1920m, rub.RatePerMdlUnit);

            var mdl = loadResult.Snapshot.GetRate("MDL");
            Assert.NotNull(mdl);
            Assert.Equal(1m, mdl.RatePerMdlUnit);
        }
        finally
        {
            if (File.Exists(tempCachePath)) File.Delete(tempCachePath);
        }
    }
}
