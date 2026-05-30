using System.Windows;
using System.Windows.Controls;
using Spendly.Desktop.ViewModels;

namespace Spendly.Desktop.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is SettingsViewModel vm)
                vm.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(SettingsViewModel.PasswordSavedMessage)
                        && string.IsNullOrEmpty(vm.PasswordSavedMessage))
                    {
                        CurrentPasswordBox.Clear();
                        NewPasswordBox.Clear();
                        ConfirmPasswordBox.Clear();
                    }
                };
        };
    }

    private void CurrentPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm) vm.CurrentPassword = CurrentPasswordBox.Password;
    }

    private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm) vm.NewPassword = NewPasswordBox.Password;
    }

    private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm) vm.ConfirmNewPassword = ConfirmPasswordBox.Password;
    }
}
