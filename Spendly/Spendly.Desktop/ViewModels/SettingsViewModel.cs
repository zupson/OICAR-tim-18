using System.Net;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spendly.Desktop.Models;
using Spendly.Desktop.Services;

namespace Spendly.Desktop.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService        _service;
    private readonly DataCache              _data;
    private readonly ApiService             _api;
    private readonly NotificationsViewModel _notifications;
    private readonly string                 _savedApiUrl;

    // ── Tab navigation ────────────────────────────────────────────────────────
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsProfilTab), nameof(IsSigurnostTab), nameof(IsObavijestTab), nameof(IsValutaTab))]
    private string _selectedTab = "Profil";

    public bool IsProfilTab    => SelectedTab == "Profil";
    public bool IsSigurnostTab => SelectedTab == "Sigurnost";
    public bool IsObavijestTab => SelectedTab == "Obavijest";
    public bool IsValutaTab    => SelectedTab == "Valuta";

    [RelayCommand]
    private void SelectTab(string tab) => SelectedTab = tab;

    // ── App settings ──────────────────────────────────────────────────────────
    [ObservableProperty] private string _selectedCurrency = "EUR";
    [ObservableProperty] private bool _budgetWarningAlerts;
    [ObservableProperty] private bool _budgetCriticalAlerts;
    [ObservableProperty] private bool _startMinimized;
    [ObservableProperty] private string _savedMessage = string.Empty;

    public List<string> Currencies { get; } = ["EUR", "USD", "HRK"];

    // ── Profile edit ──────────────────────────────────────────────────────────
    [ObservableProperty] private string _editFirstName = string.Empty;
    [ObservableProperty] private string _editLastName  = string.Empty;
    [ObservableProperty] private string _editEmail     = string.Empty;
    [ObservableProperty] private string _editUsername  = string.Empty;
    [ObservableProperty] private string _profileError  = string.Empty;
    [ObservableProperty] private string _profileSavedMessage = string.Empty;

    // ── Password change ───────────────────────────────────────────────────────
    public string CurrentPassword    { get; set; } = string.Empty;
    public string NewPassword        { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;
    [ObservableProperty] private string _passwordError        = string.Empty;
    [ObservableProperty] private string _passwordSavedMessage = string.Empty;

    public SettingsViewModel(SettingsService service, DataCache data, ApiService api, NotificationsViewModel notifications)
    {
        _service       = service;
        _data          = data;
        _api           = api;
        _notifications = notifications;

        var s = service.Load();
        _savedApiUrl          = s.ApiUrl;
        _selectedCurrency     = s.Currency;
        _budgetWarningAlerts  = s.BudgetWarningAlerts;
        _budgetCriticalAlerts = s.BudgetCriticalAlerts;
        _startMinimized       = s.StartMinimized;
        ApplyCurrency(_selectedCurrency);

        _editFirstName = api.FirstName;
        _editLastName  = api.LastName;
        _editEmail     = api.Email;
        _editUsername  = api.Username;
    }

    private static (string Symbol, decimal Rate) CurrencyInfo(string code) => code switch
    {
        "USD" => ("$",  1.10m),
        "HRK" => ("kn", 7.53m),
        _     => ("€",  1.00m),
    };

    private void ApplyCurrency(string code)
    {
        var (symbol, rate) = CurrencyInfo(code);
        _data.CurrencySymbol = symbol;
        _data.ExchangeRate   = rate;
    }

    [RelayCommand]
    private async Task Save()
    {
        _service.Save(new AppSettings
        {
            ApiUrl               = _savedApiUrl,
            Currency             = SelectedCurrency,
            BudgetWarningAlerts  = BudgetWarningAlerts,
            BudgetCriticalAlerts = BudgetCriticalAlerts,
            StartMinimized       = StartMinimized,
        });
        ApplyCurrency(SelectedCurrency);
        _notifications.Rebuild();
        SavedMessage = "Postavke su spremljene!";
        await Task.Delay(3000);
        SavedMessage = string.Empty;
    }

    [RelayCommand]
    private async Task SaveProfile()
    {
        ProfileError        = string.Empty;
        ProfileSavedMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(EditFirstName) || string.IsNullOrWhiteSpace(EditLastName) ||
            string.IsNullOrWhiteSpace(EditEmail)     || string.IsNullOrWhiteSpace(EditUsername))
        {
            ProfileError = "Sva polja su obavezna.";
            return;
        }

        try
        {
            await _api.PutAsync("/api/User/EditPerson",
                new { firstName = EditFirstName, lastName = EditLastName,
                      email = EditEmail, username = EditUsername });

            _api.FirstName = EditFirstName;
            _api.LastName  = EditLastName;
            _api.Email     = EditEmail;
            _api.Username  = EditUsername;

            ProfileSavedMessage = "Profil je ažuriran!";
            await Task.Delay(3000);
            ProfileSavedMessage = string.Empty;
        }
        catch (HttpRequestException ex)
        {
            ProfileError = ex.StatusCode == HttpStatusCode.Conflict
                ? "Korisničko ime ili email već postoje."
                : "Greška pri spremanju profila.";
        }
        catch (Exception)
        {
            ProfileError = "Greška na poslužitelju. Pokušajte ponovo.";
        }
    }

    [RelayCommand]
    private async Task ChangePassword()
    {
        PasswordError        = string.Empty;
        PasswordSavedMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(CurrentPassword)) { PasswordError = "Unesite trenutnu lozinku."; return; }
        if (string.IsNullOrWhiteSpace(NewPassword))     { PasswordError = "Unesite novu lozinku."; return; }
        if (NewPassword.Length < 8)                     { PasswordError = "Nova lozinka mora imati najmanje 8 znakova."; return; }
        if (NewPassword != ConfirmNewPassword)           { PasswordError = "Lozinke se ne podudaraju."; return; }

        try
        {
            await _api.PutAsync("/api/User/ChangePassword",
                new { currentPassword = CurrentPassword, password = NewPassword });

            CurrentPassword    = string.Empty;
            NewPassword        = string.Empty;
            ConfirmNewPassword = string.Empty;
            PasswordSavedMessage = "Lozinka je promijenjena!";
            await Task.Delay(3000);
            PasswordSavedMessage = string.Empty;
        }
        catch (HttpRequestException)
        {
            PasswordError = "Trenutna lozinka nije ispravna.";
        }
        catch (Exception)
        {
            PasswordError = "Greška na poslužitelju. Pokušajte ponovo.";
        }
    }
}
