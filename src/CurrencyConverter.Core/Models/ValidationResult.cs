namespace CurrencyConverter.Core.Models;

public record ValidationResult
{
    public bool IsValid => Request != null;
    public ConversionRequest? Request { get; }
    public string? ErrorMessage { get; }

    public static ValidationResult Success(ConversionRequest request) =>
        new(request ?? throw new ArgumentNullException(nameof(request)), null);

    public static ValidationResult Failure(string errorMessage) =>
        new(null, string.IsNullOrWhiteSpace(errorMessage) ? "Invalid input." : errorMessage);

    private ValidationResult(ConversionRequest? request, string? errorMessage)
    {
        Request = request;
        ErrorMessage = errorMessage;
    }
}
