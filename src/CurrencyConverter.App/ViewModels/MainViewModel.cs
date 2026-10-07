using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CurrencyConverter.App.Services;
using CurrencyConverter.Core.Models;
using CurrencyConverter.Core.Services;
using CurrencyConverter.Core.Validation;

namespace CurrencyConverter.App.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IRatesProvider _ratesProvider;
    private readonly IInputValidator _validator;
    private readonly ICurrencyConverter _converter;
    private readonly Func<DateOnly>? _currentDateProvider;

    private string _amount = string.Empty;
    private Currency? _selectedSourceCurrency;
    private Currency? _selectedTargetCurrency;
    private string _resultText = string.Empty;
    private string _rateDateText = "Rate Date: N/A";
    private string _sourceNameText = $"Source: {BnmConstants.SourceName}";
    private string _statusMessage = "Ready.";
    private string _validationMessage = string.Empty;
    private bool _isBusy;
    private RateSnapshot? _currentSnapshot;
    private bool _usedFallback;

    public MainViewModel(
        IRatesProvider ratesProvider,
        IInputValidator validator,
        ICurrencyConverter converter,
        Func<DateOnly>? currentDateProvider = null)
    {
        _ratesProvider = ratesProvider ?? throw new ArgumentNullException(nameof(ratesProvider));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        _currentDateProvider = currentDateProvider;

        AvailableCurrencies = new ObservableCollection<Currency>();

        ConvertCommand = new AsyncRelayCommand(ExecuteConvertAsync, CanExecuteConvert);
        RefreshRatesCommand = new AsyncRelayCommand(async () => await LoadRatesAsync(), () => !IsBusy);
    }

    public ObservableCollection<Currency> AvailableCurrencies { get; }

    public string Amount
    {
        get => _amount;
        set
        {
            if (_amount != value)
            {
                _amount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanConvert));
                ((AsyncRelayCommand)ConvertCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public Currency? SelectedSourceCurrency
    {
        get => _selectedSourceCurrency;
        set
        {
            if (_selectedSourceCurrency != value)
            {
                _selectedSourceCurrency = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanConvert));
                ((AsyncRelayCommand)ConvertCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public Currency? SelectedTargetCurrency
    {
        get => _selectedTargetCurrency;
        set
        {
            if (_selectedTargetCurrency != value)
            {
                _selectedTargetCurrency = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanConvert));
                ((AsyncRelayCommand)ConvertCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string ResultText
    {
        get => _resultText;
        set
        {
            if (_resultText != value)
            {
                _resultText = value;
                OnPropertyChanged();
            }
        }
    }

    public string RateDateText
    {
        get => _rateDateText;
        set
        {
            if (_rateDateText != value)
            {
                _rateDateText = value;
                OnPropertyChanged();
            }
        }
    }

    public string SourceNameText
    {
        get => _sourceNameText;
        set
        {
            if (_sourceNameText != value)
            {
                _sourceNameText = value;
                OnPropertyChanged();
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public string ValidationMessage
    {
        get => _validationMessage;
        set
        {
            if (_validationMessage != value)
            {
                _validationMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasValidationError));
            }
        }
    }

    public bool HasValidationError => !string.IsNullOrWhiteSpace(_validationMessage);

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy != value)
            {
                _isBusy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanConvert));
                ((AsyncRelayCommand)ConvertCommand).RaiseCanExecuteChanged();
                ((AsyncRelayCommand)RefreshRatesCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanConvert => CanExecuteConvert();

    public ICommand ConvertCommand { get; }
    public ICommand RefreshRatesCommand { get; }

    public RateSnapshot? CurrentSnapshot => _currentSnapshot;

    public bool CanExecuteConvert()
    {
        return !IsBusy &&
               !string.IsNullOrWhiteSpace(Amount) &&
               SelectedSourceCurrency != null &&
               SelectedTargetCurrency != null;
    }

    public async Task LoadRatesAsync(DateOnly? targetDate = null)
    {
        IsBusy = true;
        StatusMessage = "Loading exchange rates...";
        ValidationMessage = string.Empty;

        var date = targetDate ?? _currentDateProvider?.Invoke() ?? DateOnly.FromDateTime(DateTime.Today);

        try
        {
            var outcome = await _ratesProvider.GetLatestAsync(date);
            if (outcome.IsSuccess && outcome.Snapshot != null)
            {
                _currentSnapshot = outcome.Snapshot;
                _usedFallback = outcome.UsedFallback;

                UpdateCurrenciesList(outcome.Snapshot);

                RateDateText = $"Rate Date: {outcome.Snapshot.RateDate.ToString(BnmConstants.DateFormat)}";
                SourceNameText = $"Source: {outcome.Snapshot.SourceName}";

                if (outcome.UsedFallback)
                {
                    StatusMessage = $"Using fallback rates from {outcome.Snapshot.RateDate.ToString(BnmConstants.DateFormat)} (requested {date.ToString(BnmConstants.DateFormat)}).";
                }
                else
                {
                    StatusMessage = $"Exchange rates loaded successfully for {outcome.Snapshot.RateDate.ToString(BnmConstants.DateFormat)}.";
                }
            }
            else
            {
                StatusMessage = outcome.FailureReason ?? "Exchange rates are currently unavailable.";
                RateDateText = "Rate Date: N/A";
                SourceNameText = $"Source: {BnmConstants.SourceName}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Unexpected error loading rates: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void UpdateCurrenciesList(RateSnapshot snapshot)
    {
        var previousSource = SelectedSourceCurrency?.Code;
        var previousTarget = SelectedTargetCurrency?.Code;

        AvailableCurrencies.Clear();

        // Always place MDL first, then EUR, USD, then rest alphabetically
        var ordered = snapshot.Rates.Values
            .Select(r => new Currency(r.CurrencyCode, r.CurrencyName))
            .OrderBy(c => c.Code == "MDL" ? 0 : c.Code == "EUR" ? 1 : c.Code == "USD" ? 2 : 3)
            .ThenBy(c => c.Code)
            .ToList();

        foreach (var c in ordered)
        {
            AvailableCurrencies.Add(c);
        }

        if (previousSource != null)
        {
            SelectedSourceCurrency = AvailableCurrencies.FirstOrDefault(c => string.Equals(c.Code, previousSource, StringComparison.OrdinalIgnoreCase));
        }

        if (previousTarget != null)
        {
            SelectedTargetCurrency = AvailableCurrencies.FirstOrDefault(c => string.Equals(c.Code, previousTarget, StringComparison.OrdinalIgnoreCase));
        }

        // Set sensible defaults if still null
        if (SelectedSourceCurrency == null)
        {
            SelectedSourceCurrency = AvailableCurrencies.FirstOrDefault(c => c.Code == "EUR") ?? AvailableCurrencies.FirstOrDefault();
        }

        if (SelectedTargetCurrency == null)
        {
            SelectedTargetCurrency = AvailableCurrencies.FirstOrDefault(c => c.Code == "MDL") ?? AvailableCurrencies.LastOrDefault();
        }
    }

    public async Task ExecuteConvertAsync()
    {
        ValidationMessage = string.Empty;

        var validation = _validator.Validate(Amount, SelectedSourceCurrency?.Code, SelectedTargetCurrency?.Code);
        if (!validation.IsValid || validation.Request == null)
        {
            ValidationMessage = validation.ErrorMessage ?? "Invalid input.";
            StatusMessage = ValidationMessage;
            ResultText = string.Empty;
            return;
        }

        if (_currentSnapshot == null)
        {
            await LoadRatesAsync();
            if (_currentSnapshot == null)
            {
                StatusMessage = "Cannot complete conversion: exchange rates are unavailable.";
                ResultText = string.Empty;
                return;
            }
        }

        var conversion = _converter.Convert(validation.Request, _currentSnapshot, _usedFallback);
        if (conversion.IsSuccess)
        {
            ResultText = $"{conversion.DisplayedAmount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)} {SelectedTargetCurrency?.Code}";
            RateDateText = $"Rate Date: {conversion.RateDate.ToString(BnmConstants.DateFormat)}";
            SourceNameText = $"Source: {conversion.SourceName}";
            StatusMessage = conversion.StatusMessage;
        }
        else
        {
            StatusMessage = conversion.ErrorMessage ?? "Conversion failed.";
            ResultText = string.Empty;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
