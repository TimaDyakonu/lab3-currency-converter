using System.Windows;
using CurrencyConverter.App.Services;
using CurrencyConverter.App.Views;

namespace CurrencyConverter.App;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var viewModel = AppComposition.CreateMainViewModel();
        var mainWindow = new MainWindow(viewModel);
        mainWindow.Show();

        await viewModel.LoadRatesAsync();
    }
}
