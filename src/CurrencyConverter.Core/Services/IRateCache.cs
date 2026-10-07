using CurrencyConverter.Core.Models;

namespace CurrencyConverter.Core.Services;

public interface IRateCache
{
    Task<CacheLoadResult> LoadAsync(CancellationToken cancellationToken = default);
    Task<CacheSaveResult> SaveAsync(RateSnapshot snapshot, CancellationToken cancellationToken = default);
}
