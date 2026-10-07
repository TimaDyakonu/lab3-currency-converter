namespace CurrencyConverter.Core.Models;

public record BnmParseResult
{
    public bool IsSuccess => Snapshot != null;
    public RateSnapshot? Snapshot { get; }
    public string? ErrorMessage { get; }

    public static BnmParseResult Success(RateSnapshot snapshot) =>
        new(snapshot ?? throw new ArgumentNullException(nameof(snapshot)), null);

    public static BnmParseResult Failure(string errorMessage) =>
        new(null, string.IsNullOrWhiteSpace(errorMessage) ? "XML parsing failed." : errorMessage);

    private BnmParseResult(RateSnapshot? snapshot, string? errorMessage)
    {
        Snapshot = snapshot;
        ErrorMessage = errorMessage;
    }
}
