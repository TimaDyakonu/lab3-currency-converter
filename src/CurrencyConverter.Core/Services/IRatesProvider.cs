using CurrencyConverter.Core.Models;

namespace CurrencyConverter.Core.Services;

public interface IRatesProvider
{
    Task<RateAcquisitionOutcome> GetLatestAsync(DateOnly requestedDate, CancellationToken cancellationToken = default);
}
