namespace CurrencyConverter.Core.Models;

public record ConversionRequest
{
    public decimal Amount { get; }
    public string SourceCurrency { get; }
    public string TargetCurrency { get; }

    public ConversionRequest(decimal amount, string sourceCurrency, string targetCurrency)
    {
        if (amount <= 0m)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
        if (string.IsNullOrWhiteSpace(sourceCurrency))
            throw new ArgumentException("Source currency must be specified.", nameof(sourceCurrency));
        if (string.IsNullOrWhiteSpace(targetCurrency))
            throw new ArgumentException("Target currency must be specified.", nameof(targetCurrency));

        Amount = amount;
        SourceCurrency = sourceCurrency.Trim().ToUpperInvariant();
        TargetCurrency = targetCurrency.Trim().ToUpperInvariant();
    }
}
