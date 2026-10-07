using CurrencyConverter.Core.Models;

namespace CurrencyConverter.Core.Services;

public interface ICurrencyConverter
{
    ConversionResult Convert(ConversionRequest request, RateSnapshot snapshot, bool usedFallback = false);
}
