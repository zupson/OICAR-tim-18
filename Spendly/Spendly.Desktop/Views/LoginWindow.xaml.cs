using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Spendly.Desktop.Services;
using Spendly.Desktop.ViewModels;

namespace Spendly.Desktop.Views;

public partial class LoginWindow : Window
{
    public LoginWindow(LoginViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        vm.LoginSucceeded += async (fullName) =>
        {
            vm.IsLoading = true;
            bool loadFailed = false;
            try
            {
                var api  = App.Services.GetRequiredService<ApiService>();
                var data = App.Services.GetRequiredService<DataCache>();
                await data.LoadFromApiAsync(api);
            }
            catch (Exception ex)
            {
                loadFailed = true;
                vm.DataLoadWarning = "Upozorenje: podaci nisu mogli biti učitani.";
                System.Diagnostics.Debug.WriteLine($"Data load failed: {ex.Message}");
            }
            if (loadFailed) await Task.Delay(1500);
            vm.DataLoadWarning = string.Empty;
            vm.IsLoading = false;

            var mainWindow = App.Services.GetRequiredService<MainWindow>();
            ((MainViewModel)mainWindow.DataContext).LoggedInUser = fullName;

            var settings = App.Services.GetRequiredService<SettingsService>().Load();
            mainWindow.Show();
            if (settings.StartMinimized)
                mainWindow.WindowState = WindowState.Minimized;

            Close();
        };
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
            vm.Password = PasswordBox.Password;
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is LoginViewModel vm)
            vm.LoginCommand.Execute(null);
    }

    private void Register_Click(object sender, RoutedEventArgs e)
    {
        App.Services.GetRequiredService<RegisterWindow>().Show();
        Close();
    }
}
