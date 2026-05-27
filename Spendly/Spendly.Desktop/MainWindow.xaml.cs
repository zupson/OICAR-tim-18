using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Spendly.Desktop.Services;
using Spendly.Desktop.ViewModels;
using Spendly.Desktop.Views;

namespace Spendly.Desktop;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        Action? logoutHandler = null;
        logoutHandler = () =>
        {
            vm.LogoutRequested -= logoutHandler;
            App.Services.GetRequiredService<ApiService>().Reset();
            App.Services.GetRequiredService<DataCache>().Reset();
            App.Services.GetRequiredService<LoginWindow>().Show();
            Close();
        };
        vm.LogoutRequested += logoutHandler;
    }
}
