namespace CurrencyConverter.Core.Models;

public record RateAcquisitionOutcome
{
    public bool IsSuccess => Snapshot != null;
    public RateSnapshot? Snapshot { get; }
    public bool UsedFallback { get; }
    public DateOnly RequestedDate { get; }
    public string? FailureReason { get; }
    public string? StatusMessage { get; }

    public static RateAcquisitionOutcome Success(
        RateSnapshot snapshot,
        DateOnly requestedDate,
        bool usedFallback,
        string? statusMessage = null)
    {
        return new RateAcquisitionOutcome(
            snapshot: snapshot ?? throw new ArgumentNullException(nameof(snapshot)),
            usedFallback: usedFallback,
            requestedDate: requestedDate,
            failureReason: null,
            statusMessage: statusMessage ?? string.Empty);
    }

    public static RateAcquisitionOutcome Failure(
        DateOnly requestedDate,
        string failureReason)
    {
        return new RateAcquisitionOutcome(
            snapshot: null,
            usedFallback: false,
            requestedDate: requestedDate,
            failureReason: failureReason,
            statusMessage: failureReason);
    }

    private RateAcquisitionOutcome(
        RateSnapshot? snapshot,
        bool usedFallback,
        DateOnly requestedDate,
        string? failureReason,
        string? statusMessage)
    {
        Snapshot = snapshot;
        UsedFallback = usedFallback;
        RequestedDate = requestedDate;
        FailureReason = failureReason;
        StatusMessage = statusMessage;
    }
}
