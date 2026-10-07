using System.Net.Http;
using CurrencyConverter.App.ViewModels;
using CurrencyConverter.Core.Parsing;
using CurrencyConverter.Core.Services;
using CurrencyConverter.Core.Storage;
using CurrencyConverter.Core.Validation;

namespace CurrencyConverter.App.Services;

public static class AppComposition
{
    private static readonly HttpClient SharedHttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(BnmConstants.DefaultTimeoutSeconds)
    };

    public static MainViewModel CreateMainViewModel()
    {
        var parser = new BnmXmlParser();
        var cache = new JsonRateCache();
        var provider = new BnmRatesProvider(SharedHttpClient, parser, cache);
        var validator = new InputValidator();
        var converter = new Core.Services.CurrencyConverter();

        return new MainViewModel(provider, validator, converter);
    }
}
