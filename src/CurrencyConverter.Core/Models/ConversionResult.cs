namespace CurrencyConverter.Core.Models;

public record ConversionResult
{
    public bool IsSuccess { get; }
    public decimal Amount { get; }
    public decimal DisplayedAmount { get; }
    public DateOnly RateDate { get; }
    public string SourceName { get; }
    public bool UsedFallback { get; }
    public string StatusMessage { get; }
    public string? ErrorMessage { get; }

    public static ConversionResult Success(
        decimal amount,
        DateOnly rateDate,
        string sourceName,
        bool usedFallback,
        string statusMessage)
    {
        var displayed = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        return new ConversionResult(
            isSuccess: true,
            amount: amount,
            displayedAmount: displayed,
            rateDate: rateDate,
            sourceName: sourceName,
            usedFallback: usedFallback,
            statusMessage: statusMessage,
            errorMessage: null);
    }

    public static ConversionResult Failure(string errorMessage)
    {
        return new ConversionResult(
            isSuccess: false,
            amount: 0m,
            displayedAmount: 0m,
            rateDate: default,
            sourceName: string.Empty,
            usedFallback: false,
            statusMessage: errorMessage,
            errorMessage: errorMessage);
    }

    private ConversionResult(
        bool isSuccess,
        decimal amount,
        decimal displayedAmount,
        DateOnly rateDate,
        string sourceName,
        bool usedFallback,
        string statusMessage,
        string? errorMessage)
    {
        IsSuccess = isSuccess;
        Amount = amount;
        DisplayedAmount = displayedAmount;
        RateDate = rateDate;
        SourceName = sourceName;
        UsedFallback = usedFallback;
        StatusMessage = statusMessage;
        ErrorMessage = errorMessage;
    }
}
