using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spendly.Desktop.Models;
using Spendly.Desktop.Services;

namespace Spendly.Desktop.ViewModels;

public record BudgetCategoryRow(string Category, decimal Amount, double BarPercent);

public partial class BudgetRowViewModel : ObservableObject
{
    public Budget Budget { get; }
    private readonly DataCache _data;

    public BudgetRowViewModel(Budget budget, DataCache data)
    {
        Budget = budget;
        _data  = data;
        budget.PropertyChanged += (_, _) => OnPropertyChanged(string.Empty);
        _data.Transactions.CollectionChanged += OnTransactionsChanged;
        _data.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(DataCache.CurrencySymbol) or nameof(DataCache.ExchangeRate))
            {
                OnPropertyChanged(nameof(CurrencySymbol));
                OnPropertyChanged(nameof(ExchangeRate));
            }
        };
    }

    // Proxy Budget display properties so existing XAML bindings stay unchanged
    public int           Id                   => Budget.Id;
    public string        Name                 => Budget.Name;
    public string        Category             => Budget.Category;
    public decimal       Limit                => Budget.Limit;
    public decimal       Spent                => Budget.Spent;
    public bool          IsEditing            => Budget.IsEditing;
    public bool          IsNotEditing         => Budget.IsNotEditing;
    public decimal       EditLimit            { get => Budget.EditLimit; set => Budget.EditLimit = value; }
    public bool          IsConfirmingDelete   => Budget.IsConfirmingDelete;
    public bool          IsNotConfirmingDelete => Budget.IsNotConfirmingDelete;
    public decimal       Remaining            => Budget.Remaining;
    public decimal       RemainingAbsolute    => Budget.RemainingAbsolute;
    public bool          IsOverBudget         => Budget.IsOverBudget;
    public string        RemainingLabel       => Budget.RemainingLabel;
    public double        ProgressPercent      => Budget.ProgressPercent;
    public AlertLevel    AlertLevel           => Budget.AlertLevel;
    public int?          ApiMonth             => Budget.ApiMonth;
    public int?          ApiYear              => Budget.ApiYear;
    public int?          ApiUserGroupId       => Budget.ApiUserGroupId;
    public string        CurrencySymbol       => _data.CurrencySymbol;
    public decimal       ExchangeRate         => _data.ExchangeRate;

    // Transactions for this budget's month/year
    public IEnumerable<Transaction> RecentTransactions =>
        _data.Transactions
            .Where(t => t.Type  == TransactionType.Expense &&
                        t.Scope == TransactionScope.Personal &&
                        t.Date.Month == (Budget.ApiMonth ?? -1) &&
                        t.Date.Year  == (Budget.ApiYear  ?? -1))
            .OrderByDescending(t => t.Date)
            .Take(5);

    public IEnumerable<BudgetCategoryRow> CategorySpending
    {
        get
        {
            var groups = _data.Transactions
                .Where(t => t.Type  == TransactionType.Expense &&
                            t.Scope == TransactionScope.Personal &&
                            t.Date.Month == (Budget.ApiMonth ?? -1) &&
                            t.Date.Year  == (Budget.ApiYear  ?? -1))
                .GroupBy(t => t.Category)
                .Select(g => (Category: g.Key, Total: g.Sum(t => t.Amount)))
                .OrderByDescending(x => x.Total)
                .ToList();

            if (groups.Count == 0) return [];
            var max = (double)groups.Max(x => x.Total);
            return groups.Select(x =>
                new BudgetCategoryRow(x.Category, x.Total, max > 0 ? (double)x.Total / max * 100 : 0));
        }
    }

    public bool HasNoRecentTransactions => !RecentTransactions.Any();
    public bool HasNoCategorySpending   => !CategorySpending.Any();

    private void OnTransactionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(RecentTransactions));
        OnPropertyChanged(nameof(CategorySpending));
        OnPropertyChanged(nameof(HasNoRecentTransactions));
        OnPropertyChanged(nameof(HasNoCategorySpending));
    }
}

public partial class BudgetViewModel : ObservableObject
{
    private readonly DataCache  _data;
    private readonly ApiService _api;

    public ObservableCollection<BudgetRowViewModel> Budgets { get; } = [];

    [ObservableProperty] private bool    _isAddFormOpen;
    [ObservableProperty] private int     _newYear       = DateTime.Now.Year;
    [ObservableProperty] private int     _newMonthIndex = DateTime.Now.Month - 1;
    [ObservableProperty] private decimal _newLimit;
    [ObservableProperty] private string  _addError      = string.Empty;

    public string[] MonthNames { get; } =
        ["Siječanj", "Veljača", "Ožujak", "Travanj", "Svibanj", "Lipanj",
         "Srpanj", "Kolovoz", "Rujan", "Listopad", "Studeni", "Prosinac"];

    public string  CurrencySymbol => _data.CurrencySymbol;
    public decimal ExchangeRate   => _data.ExchangeRate;
    public bool    HasNoBudgets   => Budgets.Count == 0;

    private static readonly int _thisMonth = DateTime.Now.Month;
    private static readonly int _thisYear  = DateTime.Now.Year;

    public IEnumerable<Transaction> FamilyRecentTransactions =>
        _data.Transactions
            .Where(t => t.Type  == TransactionType.Expense &&
                        t.Scope == TransactionScope.Family &&
                        t.Date.Month == _thisMonth &&
                        t.Date.Year  == _thisYear)
            .OrderByDescending(t => t.Date)
            .Take(5);

    public IEnumerable<BudgetCategoryRow> FamilyCategorySpending
    {
        get
        {
            var groups = _data.Transactions
                .Where(t => t.Type  == TransactionType.Expense &&
                            t.Scope == TransactionScope.Family &&
                            t.Date.Month == _thisMonth &&
                            t.Date.Year  == _thisYear)
                .GroupBy(t => t.Category)
                .Select(g => (Category: g.Key, Total: g.Sum(t => t.Amount)))
                .OrderByDescending(x => x.Total)
                .ToList();

            if (groups.Count == 0) return [];
            var max = (double)groups.Max(x => x.Total);
            return groups.Select(x =>
                new BudgetCategoryRow(x.Category, x.Total, max > 0 ? (double)x.Total / max * 100 : 0));
        }
    }

    public bool HasNoFamilyTransactions   => !FamilyRecentTransactions.Any();
    public bool HasNoFamilyCategorySpending => !FamilyCategorySpending.Any();

    public BudgetViewModel(DataCache data, ApiService api)
    {
        _data = data;
        _api  = api;
        _data.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CurrencySymbol));
            OnPropertyChanged(nameof(ExchangeRate));
        };

        foreach (var b in _data.Budgets)
            Budgets.Add(new BudgetRowViewModel(b, _data));

        _data.Budgets.CollectionChanged += OnDataBudgetsChanged;
        Budgets.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasNoBudgets));
        _data.Transactions.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(FamilyRecentTransactions));
            OnPropertyChanged(nameof(FamilyCategorySpending));
            OnPropertyChanged(nameof(HasNoFamilyTransactions));
            OnPropertyChanged(nameof(HasNoFamilyCategorySpending));
        };
    }

    private void OnDataBudgetsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            foreach (Budget b in e.NewItems)
                Budgets.Add(new BudgetRowViewModel(b, _data));

        if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems != null)
            foreach (Budget b in e.OldItems)
            {
                var row = Budgets.FirstOrDefault(r => r.Budget == b);
                if (row != null) Budgets.Remove(row);
            }

        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            Budgets.Clear();
            foreach (var b in _data.Budgets)
                Budgets.Add(new BudgetRowViewModel(b, _data));
        }
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
    private void RequestDeleteBudget(BudgetRowViewModel row)
    {
        foreach (var b in Budgets) b.Budget.IsConfirmingDelete = false;
        row.Budget.IsConfirmingDelete = true;
    }

    [RelayCommand]
    private async Task ConfirmDeleteBudget(BudgetRowViewModel row)
    {
        var budget = row.Budget;
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
    private void CancelDeleteBudget(BudgetRowViewModel row) => row.Budget.IsConfirmingDelete = false;

    [RelayCommand]
    private void StartEdit(BudgetRowViewModel row)
    {
        foreach (var b in Budgets) b.Budget.IsEditing = false;
        row.Budget.EditLimit = row.Budget.Limit;
        row.Budget.IsEditing = true;
    }

    [RelayCommand]
    private async Task SaveEdit(BudgetRowViewModel row)
    {
        var budget = row.Budget;
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
    private void CancelEdit(BudgetRowViewModel row) => row.Budget.IsEditing = false;
}
