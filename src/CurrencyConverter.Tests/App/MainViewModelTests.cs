using CurrencyConverter.App.ViewModels;
using CurrencyConverter.Core.Models;
using CurrencyConverter.Core.Services;
using CurrencyConverter.Core.Validation;
using CurrencyConverter.Tests.Fixtures;

namespace CurrencyConverter.Tests.App;

public class MainViewModelTests
{
    private class FakeRatesProvider : IRatesProvider
    {
        public int CallCount { get; private set; }
        public RateAcquisitionOutcome OutcomeToReturn { get; set; }

        public FakeRatesProvider(RateAcquisitionOutcome outcome)
        {
            OutcomeToReturn = outcome;
        }

        public Task<RateAcquisitionOutcome> GetLatestAsync(DateOnly requestedDate, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(OutcomeToReturn);
        }
    }

    private readonly RateSnapshot _testSnapshot;
    private readonly FakeRatesProvider _fakeProvider;
    private readonly InputValidator _validator = new();
    private readonly Core.Services.CurrencyConverter _converter = new();

    public MainViewModelTests()
    {
        _testSnapshot = TestSnapshotBuilder.CreateValidSnapshot(new DateOnly(2026, 10, 7));
        _fakeProvider = new FakeRatesProvider(RateAcquisitionOutcome.Success(_testSnapshot, new DateOnly(2026, 10, 7), false));
    }

    [Fact]
    public void CanConvert_DisabledWhenAmountOrCurrenciesMissing()
    {
        var vm = new MainViewModel(_fakeProvider, _validator, _converter);

        // Initially amount is empty
        Assert.False(vm.CanConvert);
        Assert.False(vm.ConvertCommand.CanExecute(null));

        // Set amount, but no currencies selected yet
        vm.Amount = "100";
        Assert.False(vm.CanConvert);

        // Select source only
        vm.SelectedSourceCurrency = new Currency("EUR", "Euro");
        Assert.False(vm.CanConvert);

        // Select target
        vm.SelectedTargetCurrency = new Currency("MDL", "Moldovan Leu");
        Assert.True(vm.CanConvert);
        Assert.True(vm.ConvertCommand.CanExecute(null));

        // Clear amount
        vm.Amount = "";
        Assert.False(vm.CanConvert);
    }

    [Fact]
    public async Task Convert_InvalidInput_ShowsValidationMessageAndDoesNotProduceResult()
    {
        var vm = new MainViewModel(_fakeProvider, _validator, _converter);
        await vm.LoadRatesAsync();

        var initialCalls = _fakeProvider.CallCount;

        vm.Amount = "abc";
        vm.SelectedSourceCurrency = vm.AvailableCurrencies.First(c => c.Code == "EUR");
        vm.SelectedTargetCurrency = vm.AvailableCurrencies.First(c => c.Code == "MDL");

        await vm.ExecuteConvertAsync();

        Assert.True(vm.HasValidationError);
        Assert.Contains("valid numeric", vm.ValidationMessage);
        Assert.Empty(vm.ResultText);
        Assert.Equal(initialCalls, _fakeProvider.CallCount); // Provider was not invoked during invalid conversion
    }

    [Fact]
    public async Task Convert_ZeroOrNegative_ShowsValidationMessage()
    {
        var vm = new MainViewModel(_fakeProvider, _validator, _converter);
        await vm.LoadRatesAsync();

        vm.Amount = "0";
        vm.SelectedSourceCurrency = vm.AvailableCurrencies.First(c => c.Code == "EUR");
        vm.SelectedTargetCurrency = vm.AvailableCurrencies.First(c => c.Code == "MDL");

        await vm.ExecuteConvertAsync();

        Assert.True(vm.HasValidationError);
        Assert.Contains("greater than zero", vm.ValidationMessage);
        Assert.Empty(vm.ResultText);

        vm.Amount = "-50";
        await vm.ExecuteConvertAsync();

        Assert.True(vm.HasValidationError);
        Assert.Contains("greater than zero", vm.ValidationMessage);
        Assert.Empty(vm.ResultText);
    }

    [Theory]
    [InlineData("10.50")]
    [InlineData("10,50")]
    public async Task Convert_DotAndCommaSeparators_BothSucceed(string amountText)
    {
        var vm = new MainViewModel(_fakeProvider, _validator, _converter);
        await vm.LoadRatesAsync();

        vm.Amount = amountText;
        vm.SelectedSourceCurrency = vm.AvailableCurrencies.First(c => c.Code == "EUR");
        vm.SelectedTargetCurrency = vm.AvailableCurrencies.First(c => c.Code == "MDL");

        await vm.ExecuteConvertAsync();

        Assert.False(vm.HasValidationError);
        Assert.NotEmpty(vm.ResultText);
        Assert.Contains("204.75 MDL", vm.ResultText); // 10.50 * 19.50 = 204.75
        Assert.Contains("07.10.2026", vm.RateDateText);
        Assert.Contains(BnmConstants.SourceName, vm.SourceNameText);
    }

    [Fact]
    public async Task Convert_ValidFlow_UpdatesVisibleBindings()
    {
        var vm = new MainViewModel(_fakeProvider, _validator, _converter);
        await vm.LoadRatesAsync();

        vm.Amount = "100";
        vm.SelectedSourceCurrency = vm.AvailableCurrencies.First(c => c.Code == "EUR");
        vm.SelectedTargetCurrency = vm.AvailableCurrencies.First(c => c.Code == "USD");

        await vm.ExecuteConvertAsync();

        Assert.False(vm.HasValidationError);
        Assert.Equal("109.55 USD", vm.ResultText);
        Assert.Equal("Rate Date: 07.10.2026", vm.RateDateText);
        Assert.Equal($"Source: {BnmConstants.SourceName}", vm.SourceNameText);
        Assert.Contains("Converted using official BNM rates", vm.StatusMessage);
    }
}
