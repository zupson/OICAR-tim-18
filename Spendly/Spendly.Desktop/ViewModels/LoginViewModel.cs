using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Spendly.Desktop.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;

    public event Action? LoginSucceeded;

    // Hardcoded credentials — replace with API call later
    private static readonly (string User, string Pass)[] ValidUsers =
    [
        ("user",  "user123"),
        ("admin", "Admin123"),
    ];

    [RelayCommand]
    private void Login()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Unesite korisničko ime i lozinku.";
            return;
        }

        if (!ValidUsers.Any(u => u.User == Username && u.Pass == Password))
        {
            ErrorMessage = "Pogrešno korisničko ime ili lozinka.";
            return;
        }

        ErrorMessage = string.Empty;
        LoginSucceeded?.Invoke();
    }
}
