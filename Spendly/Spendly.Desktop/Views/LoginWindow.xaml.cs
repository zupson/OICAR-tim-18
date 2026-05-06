using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Spendly.Desktop.ViewModels;

namespace Spendly.Desktop.Views;

public partial class LoginWindow : Window
{
    public LoginWindow(LoginViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;

        vm.LoginSucceeded += () =>
        {
            App.Services.GetRequiredService<MainWindow>().Show();
            Close();
        };
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel vm)
            vm.Password = PasswordBox.Password;
    }
}
