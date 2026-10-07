using System.Windows;
using CurrencyConverter.App.ViewModels;

namespace CurrencyConverter.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(MainViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
