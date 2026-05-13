using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spendly.Desktop.Models;
using Spendly.Desktop.Services;

namespace Spendly.Desktop.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _service;

    [ObservableProperty] private string _apiUrl = string.Empty;
    [ObservableProperty] private string _selectedCurrency = "EUR";
    [ObservableProperty] private bool _budgetWarningAlerts;
    [ObservableProperty] private bool _budgetCriticalAlerts;
    [ObservableProperty] private bool _startMinimized;
    [ObservableProperty] private string _savedMessage = string.Empty;

    public List<string> Currencies { get; } = ["EUR", "USD", "HRK"];

    public SettingsViewModel(SettingsService service)
    {
        _service = service;
        var s = service.Load();
        _apiUrl             = s.ApiUrl;
        _selectedCurrency   = s.Currency;
        _budgetWarningAlerts  = s.BudgetWarningAlerts;
        _budgetCriticalAlerts = s.BudgetCriticalAlerts;
        _startMinimized     = s.StartMinimized;
    }

    [RelayCommand]
    private void Save()
    {
        _service.Save(new AppSettings
        {
            ApiUrl               = ApiUrl,
            Currency             = SelectedCurrency,
            BudgetWarningAlerts  = BudgetWarningAlerts,
            BudgetCriticalAlerts = BudgetCriticalAlerts,
            StartMinimized       = StartMinimized,
        });
        SavedMessage = "Postavke su spremljene!";
    }
}
