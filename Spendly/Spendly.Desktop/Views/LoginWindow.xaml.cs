using System.Windows;
using System.Windows.Input;
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
            var mainWindow = App.Services.GetRequiredService<MainWindow>();
            ((MainViewModel)mainWindow.DataContext).LoggedInUser = vm.Username;
            mainWindow.Show();
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
}
