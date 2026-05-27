using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Spendly.Desktop.Services;
using Spendly.Desktop.ViewModels;

namespace Spendly.Desktop.Views;

public partial class RegisterWindow : Window
{
    public RegisterWindow(RegisterViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        vm.RegisterSucceeded += async (fullName) =>
        {
            vm.IsLoading = true;
            try
            {
                var api  = App.Services.GetRequiredService<ApiService>();
                var data = App.Services.GetRequiredService<DataCache>();
                await data.LoadFromApiAsync(api);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Data load failed: {ex.Message}");
            }
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
        if (DataContext is RegisterViewModel vm)
            vm.Password = PasswordBox.Password;
    }

    private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is RegisterViewModel vm)
            vm.ConfirmPassword = ConfirmPasswordBox.Password;
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is RegisterViewModel vm)
            vm.RegisterCommand.Execute(null);
    }

    private void BackToLogin_Click(object sender, RoutedEventArgs e)
    {
        App.Services.GetRequiredService<LoginWindow>().Show();
        Close();
    }
}
