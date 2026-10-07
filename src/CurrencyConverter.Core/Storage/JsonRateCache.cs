using System.Text.Json;
using CurrencyConverter.Core.Models;
using CurrencyConverter.Core.Services;

namespace CurrencyConverter.Core.Storage;

public class JsonRateCache : IRateCache
{
    private readonly string _cacheFilePath;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public JsonRateCache(string? customFilePath = null)
    {
        if (!string.IsNullOrWhiteSpace(customFilePath))
        {
            _cacheFilePath = customFilePath;
        }
        else
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appDir = Path.Combine(appData, "BnmCurrencyConverter");
            _cacheFilePath = Path.Combine(appDir, "rates_cache.json");
        }
    }

    public string CacheFilePath => _cacheFilePath;

    public async Task<CacheLoadResult> LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(_cacheFilePath))
            {
                return CacheLoadResult.NotFound();
            }

            var json = await File.ReadAllTextAsync(_cacheFilePath, cancellationToken);
            if (string.IsNullOrWhiteSpace(json))
            {
                return CacheLoadResult.Corrupt("Cache file is empty.");
            }

            var dto = JsonSerializer.Deserialize<RateSnapshotDto>(json, JsonOptions);
            if (dto == null || dto.RateDate == default || dto.Rates == null || dto.Rates.Count == 0)
            {
                return CacheLoadResult.Corrupt("Cache data is missing required rate snapshot fields.");
            }

            var rates = new List<ExchangeRate>();
            foreach (var r in dto.Rates)
            {
                if (string.IsNullOrWhiteSpace(r.CurrencyCode) || r.Nominal <= 0m || r.Value <= 0m)
                {
                    return CacheLoadResult.Corrupt($"Invalid rate entry in cache for currency '{r.CurrencyCode}'.");
                }

                rates.Add(new ExchangeRate(r.CurrencyCode, r.CurrencyName ?? r.CurrencyCode, r.Nominal, r.Value));
            }

            var snapshot = new RateSnapshot(
                dto.RateDate,
                string.IsNullOrWhiteSpace(dto.SourceName) ? BnmConstants.SourceName : dto.SourceName,
                rates,
                dto.RetrievedAt);

            return CacheLoadResult.Success(snapshot);
        }
        catch (JsonException ex)
        {
            return CacheLoadResult.Corrupt($"Malformed cache JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            return CacheLoadResult.Corrupt($"Error reading cache file: {ex.Message}");
        }
    }

    public async Task<CacheSaveResult> SaveAsync(RateSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        if (snapshot == null)
        {
            return CacheSaveResult.Failure("Cannot save a null rate snapshot.");
        }

        string? tempFile = null;
        try
        {
            var dir = Path.GetDirectoryName(_cacheFilePath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var dto = new RateSnapshotDto
            {
                RateDate = snapshot.RateDate,
                SourceName = snapshot.SourceName,
                RetrievedAt = snapshot.RetrievedAt,
                Rates = snapshot.Rates.Values.Select(r => new ExchangeRateDto
                {
                    CurrencyCode = r.CurrencyCode,
                    CurrencyName = r.CurrencyName,
                    Nominal = r.Nominal,
                    Value = r.Value,
                    RatePerMdlUnit = r.RatePerMdlUnit
                }).ToList()
            };

            var json = JsonSerializer.Serialize(dto, JsonOptions);

            tempFile = Path.Combine(dir ?? ".", $"{Path.GetFileName(_cacheFilePath)}.{Guid.NewGuid():N}.tmp");
            await File.WriteAllTextAsync(tempFile, json, cancellationToken);

            File.Move(tempFile, _cacheFilePath, overwrite: true);
            tempFile = null;

            return CacheSaveResult.Success();
        }
        catch (Exception ex)
        {
            return CacheSaveResult.Failure($"Failed to persist rate snapshot: {ex.Message}");
        }
        finally
        {
            if (tempFile != null && File.Exists(tempFile))
            {
                try
                {
                    File.Delete(tempFile);
                }
                catch
                {
                    // Ignore temp file cleanup failure
                }
            }
        }
    }

    private class RateSnapshotDto
    {
        public DateOnly RateDate { get; set; }
        public string? SourceName { get; set; }
        public DateTimeOffset RetrievedAt { get; set; }
        public List<ExchangeRateDto>? Rates { get; set; }
    }

    private class ExchangeRateDto
    {
        public string? CurrencyCode { get; set; }
        public string? CurrencyName { get; set; }
        public decimal Nominal { get; set; }
        public decimal Value { get; set; }
        public decimal RatePerMdlUnit { get; set; }
    }
}
