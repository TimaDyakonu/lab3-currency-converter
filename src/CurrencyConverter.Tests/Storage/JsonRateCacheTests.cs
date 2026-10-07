using CurrencyConverter.Core.Storage;

namespace CurrencyConverter.Tests.Storage;

public class JsonRateCacheTests
{
    [Fact]
    public async Task LoadAsync_NonExistentFile_ReturnsNotFound()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), $"missing_{Guid.NewGuid():N}.json");
        var cache = new JsonRateCache(missingPath);

        var result = await cache.LoadAsync();

        Assert.False(result.IsSuccess);
        Assert.True(result.IsNotFound);
    }

    [Fact]
    public async Task LoadAsync_MalformedJson_ReturnsCorruptFailureWithoutThrowing()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"corrupt_{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(tempFile, "{ this is not valid json }");
            var cache = new JsonRateCache(tempFile);

            var result = await cache.LoadAsync();

            Assert.False(result.IsSuccess);
            Assert.Contains("Malformed cache JSON", result.ErrorMessage);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadAsync_EmptyFile_ReturnsCorruptFailure()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"empty_{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(tempFile, "");
            var cache = new JsonRateCache(tempFile);

            var result = await cache.LoadAsync();

            Assert.False(result.IsSuccess);
            Assert.Contains("Cache file is empty", result.ErrorMessage);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadAsync_MissingRequiredFields_ReturnsCorruptFailure()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"missing_fields_{Guid.NewGuid():N}.json");
        try
        {
            // Missing Rates
            await File.WriteAllTextAsync(tempFile, "{\"RateDate\": \"2026-10-07\", \"SourceName\": \"BNM\"}");
            var cache = new JsonRateCache(tempFile);

            var result = await cache.LoadAsync();

            Assert.False(result.IsSuccess);
            Assert.Contains("missing required", result.ErrorMessage);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SaveAsync_NullSnapshot_ReturnsFailureWithoutThrowing()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"null_snapshot_{Guid.NewGuid():N}.json");
        var cache = new JsonRateCache(tempFile);

        var result = await cache.SaveAsync(null!);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
    }
}
