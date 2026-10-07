using CurrencyConverter.Core.Models;

namespace CurrencyConverter.Core.Services;

public class CurrencyConverter : ICurrencyConverter
{
    public ConversionResult Convert(ConversionRequest request, RateSnapshot snapshot, bool usedFallback = false)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (snapshot == null)
            return ConversionResult.Failure("No exchange rate snapshot available.");

        var sourceCode = request.SourceCurrency.Trim().ToUpperInvariant();
        var targetCode = request.TargetCurrency.Trim().ToUpperInvariant();

        // Identical currencies
        if (string.Equals(sourceCode, targetCode, StringComparison.OrdinalIgnoreCase))
        {
            var fallbackNote = usedFallback ? " (using fallback rate)" : string.Empty;
            return ConversionResult.Success(
                amount: request.Amount,
                rateDate: snapshot.RateDate,
                sourceName: snapshot.SourceName,
                usedFallback: usedFallback,
                statusMessage: $"Identical currency conversion{fallbackNote}.");
        }

        var sourceRate = snapshot.GetRate(sourceCode);
        if (sourceRate == null)
        {
            return ConversionResult.Failure($"Currency '{sourceCode}' is not available in the exchange rate snapshot.");
        }

        var targetRate = snapshot.GetRate(targetCode);
        if (targetRate == null)
        {
            return ConversionResult.Failure($"Currency '{targetCode}' is not available in the exchange rate snapshot.");
        }

        if (targetRate.RatePerMdlUnit <= 0m)
        {
            return ConversionResult.Failure($"Invalid exchange rate for '{targetCode}'.");
        }

        // Calculation: amount * sourceRate / targetRate (through MDL base)
        decimal convertedAmount = request.Amount * sourceRate.RatePerMdlUnit / targetRate.RatePerMdlUnit;

        var statusMsg = usedFallback
            ? $"Converted using fallback rates from {snapshot.RateDate.ToString(BnmConstants.DateFormat)}."
            : $"Converted using official BNM rates from {snapshot.RateDate.ToString(BnmConstants.DateFormat)}.";

        return ConversionResult.Success(
            amount: convertedAmount,
            rateDate: snapshot.RateDate,
            sourceName: snapshot.SourceName,
            usedFallback: usedFallback,
            statusMessage: statusMsg);
    }
}
