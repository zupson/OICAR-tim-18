using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spendly.Desktop.Models;
using Spendly.Desktop.Services;

namespace Spendly.Desktop.ViewModels;

public partial class BudgetViewModel : ObservableObject
{
    private readonly DataCache _data;
    private readonly ApiService      _api;

    public ObservableCollection<Budget> Budgets => _data.Budgets;

    [ObservableProperty] private bool    _isAddFormOpen;
    [ObservableProperty] private int     _newYear   = DateTime.Now.Year;
    [ObservableProperty] private int     _newMonthIndex = DateTime.Now.Month - 1;
    [ObservableProperty] private decimal _newLimit;
    [ObservableProperty] private string  _addError = string.Empty;

    public string[] MonthNames { get; } =
        ["Siječanj", "Veljača", "Ožujak", "Travanj", "Svibanj", "Lipanj",
         "Srpanj", "Kolovoz", "Rujan", "Listopad", "Studeni", "Prosinac"];

    public string  CurrencySymbol => _data.CurrencySymbol;
    public decimal ExchangeRate   => _data.ExchangeRate;
    public bool    HasNoBudgets   => _data.Budgets.Count == 0;

    public BudgetViewModel(DataCache data, ApiService api)
    {
        _data = data;
        _api  = api;
        _data.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CurrencySymbol));
            OnPropertyChanged(nameof(ExchangeRate));
        };
        _data.Budgets.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoBudgets));
    }

    [RelayCommand]
    private void ToggleAddForm()
    {
        IsAddFormOpen  = !IsAddFormOpen;
        NewYear        = DateTime.Now.Year;
        NewMonthIndex  = DateTime.Now.Month - 1;
        NewLimit       = 0;
        AddError       = string.Empty;
    }

    [RelayCommand]
    private async Task AddBudget()
    {
        if (NewYear < 2020 || NewYear > 2100) { AddError = "Unesite valjanu godinu."; return; }
        if (NewLimit <= 0)                    { AddError = "Limit mora biti veći od 0."; return; }
        if (_api.PersonalUserGroupId == 0)    { AddError = "Greška: niste prijavljeni ili podaci nisu učitani."; return; }

        int month = NewMonthIndex + 1;
        int year  = NewYear;

        try
        {
            var created = await _api.PostAsync<object, ApiBudget>(
                $"/api/Budget/CreateBudget/{_api.PersonalUserGroupId}",
                new { amount = NewLimit, year, month, currency = 0 });

            var spent = _data.Transactions
                .Where(t => t.Type == TransactionType.Expense &&
                            t.Date.Month == month && t.Date.Year == year)
                .Sum(t => t.Amount);

            _data.Budgets.Add(new Budget
            {
                Id             = created?.Id ?? 0,
                Name           = DataCache.FormatBudgetName(year, month),
                Category       = string.Empty,
                Limit          = NewLimit,
                Spent          = spent,
                ApiMonth       = month,
                ApiYear        = year,
                ApiUserGroupId = created?.UserGroupId ?? _api.PersonalUserGroupId,
            });

            IsAddFormOpen = false;
            AddError      = string.Empty;
        }
        catch (System.Net.Http.HttpRequestException ex)
        {
            AddError = ex.StatusCode == System.Net.HttpStatusCode.InternalServerError ||
                       ex.StatusCode == System.Net.HttpStatusCode.Conflict
                ? "Proračun za taj mjesec već postoji."
                : "Greška pri dodavanju proračuna. Pokušajte ponovo.";
        }
        catch (Exception)
        {
            AddError = "Greška pri dodavanju proračuna. Pokušajte ponovo.";
        }
    }

    [RelayCommand]
    private void RequestDeleteBudget(Budget budget)
    {
        foreach (var b in _data.Budgets) b.IsConfirmingDelete = false;
        budget.IsConfirmingDelete = true;
    }

    [RelayCommand]
    private async Task ConfirmDeleteBudget(Budget budget)
    {
        try
        {
            int dummyMonth = ((budget.Id - 1) % 12) + 1;
            int dummyYear  = 2000 + ((budget.Id - 1) / 12);
            await _api.PutAsync($"/api/Budget/UpdateBudget/{budget.Id}",
                new { amount = budget.Limit, year = dummyYear, month = dummyMonth, currency = 0 });
            await _api.DeleteAsync($"/api/Budget/DeleteBudget/{budget.Id}");
            _data.Budgets.Remove(budget);
        }
        catch (Exception)
        {
            budget.IsConfirmingDelete = false;
            AddError = "Greška pri brisanju proračuna. Pokušajte ponovo.";
        }
    }

    [RelayCommand]
    private void CancelDeleteBudget(Budget budget) => budget.IsConfirmingDelete = false;

    [RelayCommand]
    private void StartEdit(Budget budget)
    {
        foreach (var b in _data.Budgets) b.IsEditing = false;
        budget.EditLimit = budget.Limit;
        budget.IsEditing = true;
    }

    [RelayCommand]
    private async Task SaveEdit(Budget budget)
    {
        if (budget.EditLimit <= 0) { budget.IsEditing = false; return; }

        try
        {
            await _api.PutAsync(
                $"/api/Budget/UpdateBudget/{budget.Id}",
                new { amount = budget.EditLimit, year = budget.ApiYear ?? DateTime.Now.Year,
                      month = budget.ApiMonth ?? DateTime.Now.Month, currency = 0 });
            budget.Limit = budget.EditLimit;
        }
        catch (Exception)
        {
            AddError = "Greška pri uređivanju proračuna. Pokušajte ponovo.";
        }
        budget.IsEditing = false;
    }

    [RelayCommand]
    private void CancelEdit(Budget budget) => budget.IsEditing = false;
}
