using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spendly.Desktop.Models;
using Spendly.Desktop.Services;

namespace Spendly.Desktop.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly ApiService _api;

    [ObservableProperty] private string _username     = string.Empty;
    [ObservableProperty] private string _password     = string.Empty;
    [ObservableProperty] private string _errorMessage    = string.Empty;
    [ObservableProperty] private string _dataLoadWarning = string.Empty;
    [ObservableProperty] private bool   _isLoading;

    public event Action<string>? LoginSucceeded;

    public LoginViewModel(ApiService api) => _api = api;

    [RelayCommand]
    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Unesite korisničko ime i lozinku.";
            return;
        }

        try
        {
            var response = await _api.PostAsync<object, ApiLoginResponse>(
                "/api/User/LoginUser",
                new { username = Username, password = Password });

            if (response is null)
            {
                ErrorMessage = "Neispravan odgovor servera.";
                return;
            }

            _api.Token     = response.Token;
            _api.UserId    = response.User.Id;
            _api.FirstName = response.User.FirstName;
            _api.LastName  = response.User.LastName;
            _api.Email     = response.User.Email;
            _api.Username  = response.User.Username;

            // Login response has empty Groups — fetch them separately
            var allGroups    = await _api.GetAsync<ApiUserGroup[]>("/api/UserGroup/GetAllUserGroups") ?? [];
            var personal     = allGroups.FirstOrDefault(g => g.IsPersonal);
            var familyGroups = allGroups.Where(g => !g.IsPersonal).OrderBy(g => g.Id).ToArray();

            if (personal != null)
            {
                _api.PersonalGroupId     = personal.GroupId;
                _api.PersonalUserGroupId = personal.Id;
            }
            if (familyGroups.Length > 0)
            {
                _api.FamilyGroupId     = familyGroups[0].GroupId;
                _api.FamilyUserGroupId = familyGroups[0].Id;
            }

            ErrorMessage = string.Empty;
            LoginSucceeded?.Invoke($"{response.User.FirstName} {response.User.LastName}");
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Pogrešno korisničko ime ili lozinka.";
        }
        catch (Exception)
        {
            ErrorMessage = "Ne mogu se spojiti na server. Provjerite je li backend pokrenut.";
        }
    }
}
