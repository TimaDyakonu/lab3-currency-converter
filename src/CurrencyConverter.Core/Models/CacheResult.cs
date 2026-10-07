namespace CurrencyConverter.Core.Models;

public record CacheLoadResult
{
    public bool IsSuccess => Snapshot != null;
    public RateSnapshot? Snapshot { get; }
    public bool IsNotFound { get; }
    public string? ErrorMessage { get; }

    public static CacheLoadResult Success(RateSnapshot snapshot) =>
        new(snapshot ?? throw new ArgumentNullException(nameof(snapshot)), false, null);

    public static CacheLoadResult NotFound() =>
        new(null, true, "Cache not found.");

    public static CacheLoadResult Corrupt(string errorMessage) =>
        new(null, false, string.IsNullOrWhiteSpace(errorMessage) ? "Cache is corrupt." : errorMessage);

    private CacheLoadResult(RateSnapshot? snapshot, bool isNotFound, string? errorMessage)
    {
        Snapshot = snapshot;
        IsNotFound = isNotFound;
        ErrorMessage = errorMessage;
    }
}

public record CacheSaveResult
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }

    public static CacheSaveResult Success() => new(true, null);
    public static CacheSaveResult Failure(string errorMessage) =>
        new(false, string.IsNullOrWhiteSpace(errorMessage) ? "Failed to save cache." : errorMessage);

    private CacheSaveResult(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }
}
