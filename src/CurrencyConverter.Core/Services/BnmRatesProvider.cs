using CurrencyConverter.Core.Models;
using CurrencyConverter.Core.Parsing;

namespace CurrencyConverter.Core.Services;

public class BnmRatesProvider : IRatesProvider
{
    private readonly HttpClient _httpClient;
    private readonly IBnmXmlParser _parser;
    private readonly IRateCache _cache;
    private readonly TimeSpan _requestTimeout;

    public BnmRatesProvider(
        HttpClient httpClient,
        IBnmXmlParser parser,
        IRateCache cache,
        TimeSpan? requestTimeout = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _requestTimeout = requestTimeout ?? TimeSpan.FromSeconds(BnmConstants.DefaultTimeoutSeconds);
    }

    public async Task<RateAcquisitionOutcome> GetLatestAsync(
        DateOnly requestedDate,
        CancellationToken cancellationToken = default)
    {
        bool transportErrorOccurred = false;
        string? networkErrorMessage = null;

        // Try requested date and up to MaxFallbackDays preceding calendar days
        for (int offset = 0; offset <= BnmConstants.MaxFallbackDays; offset++)
        {
            var dateToTry = requestedDate.AddDays(-offset);
            var url = string.Format(BnmConstants.UrlTemplate, dateToTry.ToString(BnmConstants.DateFormat));

            try
            {
                using var timeoutCts = new CancellationTokenSource(_requestTimeout);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                var response = await _httpClient.GetAsync(url, linkedCts.Token);
                if (!response.IsSuccessStatusCode)
                {
                    // Non-success status (e.g. 404 for non-publication day): continue fallback search
                    continue;
                }

                var content = await response.Content.ReadAsStringAsync(linkedCts.Token);
                if (string.IsNullOrWhiteSpace(content))
                {
                    // Empty body (non-publication day): continue fallback search
                    continue;
                }

                var parseResult = _parser.Parse(content);
                if (parseResult.IsSuccess && parseResult.Snapshot != null)
                {
                    var snapshot = parseResult.Snapshot;

                    // Atomically cache the valid successful snapshot
                    await _cache.SaveAsync(snapshot, cancellationToken);

                    bool usedFallback = snapshot.RateDate != requestedDate;
                    var status = usedFallback
                        ? $"Using official BNM rates from previous publication date {snapshot.RateDate.ToString(BnmConstants.DateFormat)}."
                        : $"Rates successfully retrieved for {snapshot.RateDate.ToString(BnmConstants.DateFormat)}.";

                    return RateAcquisitionOutcome.Success(
                        snapshot: snapshot,
                        requestedDate: requestedDate,
                        usedFallback: usedFallback,
                        statusMessage: status);
                }
                // If XML failed to parse (empty elements or malformed), try previous date
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Timeout occurred
                transportErrorOccurred = true;
                networkErrorMessage = "Network request timed out.";
                break;
            }
            catch (HttpRequestException ex)
            {
                // Network transport failure
                transportErrorOccurred = true;
                networkErrorMessage = $"Network request failed: {ex.Message}";
                break;
            }
            catch (Exception ex)
            {
                transportErrorOccurred = true;
                networkErrorMessage = $"Transport error: {ex.Message}";
                break;
            }
        }

        // All network candidates failed or network transport failure occurred: fallback to cache
        try
        {
            var cacheResult = await _cache.LoadAsync(cancellationToken);
            if (cacheResult.IsSuccess && cacheResult.Snapshot != null)
            {
                var cachedSnapshot = cacheResult.Snapshot;
                var reason = transportErrorOccurred
                    ? $"Network unavailable ({networkErrorMessage}). Using saved rates from {cachedSnapshot.RateDate.ToString(BnmConstants.DateFormat)}."
                    : $"Current rates not published for requested date range. Using saved rates from {cachedSnapshot.RateDate.ToString(BnmConstants.DateFormat)}.";

                return RateAcquisitionOutcome.Success(
                    snapshot: cachedSnapshot,
                    requestedDate: requestedDate,
                    usedFallback: true,
                    statusMessage: reason);
            }
        }
        catch
        {
            // Cache failures are non-fatal
        }

        var failureMessage = transportErrorOccurred
            ? $"Network unavailable and no saved exchange rates are found ({networkErrorMessage})."
            : $"Exchange rates are unavailable for {requestedDate.ToString(BnmConstants.DateFormat)} (no rates published for past 7 days and no local cache).";

        return RateAcquisitionOutcome.Failure(requestedDate, failureMessage);
    }
}
