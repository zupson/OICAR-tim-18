using System.Net;
using System.Net.Http;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spendly.Desktop.Models;
using Spendly.Desktop.Services;

namespace Spendly.Desktop.ViewModels;

public partial class RegisterViewModel : ObservableObject
{
    private readonly ApiService _api;

    [ObservableProperty] private string _firstName       = string.Empty;
    [ObservableProperty] private string _lastName        = string.Empty;
    [ObservableProperty] private string _email           = string.Empty;
    [ObservableProperty] private string _username        = string.Empty;
    [ObservableProperty] private string _password        = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;

    [ObservableProperty] private bool _isLoading;

    // Per-field errors
    [ObservableProperty] private string _firstNameError    = string.Empty;
    [ObservableProperty] private string _lastNameError     = string.Empty;
    [ObservableProperty] private string _emailError        = string.Empty;
    [ObservableProperty] private string _usernameError     = string.Empty;
    [ObservableProperty] private string _passwordError     = string.Empty;
    [ObservableProperty] private string _confirmError      = string.Empty;
    [ObservableProperty] private string _generalError      = string.Empty;

    public event Action<string>? RegisterSucceeded;

    public RegisterViewModel(ApiService api) => _api = api;

    private void ClearErrors()
    {
        FirstNameError = LastNameError = EmailError =
        UsernameError  = PasswordError = ConfirmError = GeneralError = string.Empty;
    }

    private bool ValidateForm()
    {
        ClearErrors();
        bool ok = true;

        if (string.IsNullOrWhiteSpace(FirstName))      { FirstNameError = "Ime je obavezno.";                       ok = false; }
        if (string.IsNullOrWhiteSpace(LastName))       { LastNameError  = "Prezime je obavezno.";                   ok = false; }
        if (string.IsNullOrWhiteSpace(Email))          { EmailError     = "Email je obavezan.";                     ok = false; }
        if (string.IsNullOrWhiteSpace(Username))       { UsernameError  = "Korisničko ime je obavezno.";            ok = false; }
        if (string.IsNullOrWhiteSpace(Password))       { PasswordError  = "Lozinka je obavezna.";                   ok = false; }
        else if (Password.Length < 8)                  { PasswordError  = "Lozinka mora imati najmanje 8 znakova."; ok = false; }
        if (!string.IsNullOrEmpty(Password) && Password != ConfirmPassword)
                                                       { ConfirmError   = "Lozinke se ne podudaraju.";              ok = false; }
        return ok;
    }

    [RelayCommand]
    private async Task Register()
    {
        if (!ValidateForm()) return;

        try
        {
            var response = await _api.PostAsync<object, ApiLoginResponse>(
                "/api/User/RegisterUser",
                new { firstName = FirstName, lastName = LastName,
                      email = Email, username = Username, password = Password });

            if (response is null) { GeneralError = "Neispravan odgovor servera."; return; }

            _api.Token     = response.Token;
            _api.UserId    = response.User.Id;
            _api.FirstName = response.User.FirstName;
            _api.LastName  = response.User.LastName;
            _api.Email     = response.User.Email;
            _api.Username  = response.User.Username;

            var allGroups = await _api.GetAsync<ApiUserGroup[]>("/api/UserGroup/GetAllUserGroups") ?? [];
            var myGroups  = allGroups.Where(g => g.UserId == _api.UserId).OrderBy(g => g.Id).ToArray();
            if (myGroups.Length > 0) { _api.PersonalGroupId     = myGroups[0].GroupId; _api.PersonalUserGroupId = myGroups[0].Id; }
            if (myGroups.Length > 1) { _api.FamilyGroupId       = myGroups[1].GroupId; _api.FamilyUserGroupId   = myGroups[1].Id; }

            RegisterSucceeded?.Invoke($"{response.User.FirstName} {response.User.LastName}");
        }
        catch (HttpRequestException ex)
        {
            ApplyServerErrors(ex.Message, ex.StatusCode);
        }
        catch (Exception ex)
        {
            GeneralError = $"Greška: {ex.Message}";
        }
    }

    private void ApplyServerErrors(string body, HttpStatusCode? status)
    {
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("errors", out var errors))
            {
                foreach (var field in errors.EnumerateObject())
                {
                    var msg = string.Join(" ", field.Value.EnumerateArray()
                                                          .Select(m => m.GetString() ?? ""));
                    switch (field.Name)
                    {
                        case "FirstName": FirstNameError = "Ime mora imati između 3 i 100 znakova."; break;
                        case "LastName":  LastNameError  = "Prezime mora imati između 3 i 100 znakova."; break;
                        case "Email":    EmailError    = msg.Contains("valid") ? "Email adresa nije ispravna." : "Email mora imati najviše 200 znakova."; break;
                        case "Username": UsernameError = "Korisničko ime mora imati između 3 i 50 znakova."; break;
                        case "Password":  PasswordError  = msg; break;
                        default:          GeneralError   = msg; break;
                    }
                }
                return;
            }
        }
        catch { }

        GeneralError = status == HttpStatusCode.Conflict
            ? "Korisničko ime ili email već postoje."
            : $"Greška ({(int?)status})";
    }
}
