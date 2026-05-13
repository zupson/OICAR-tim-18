using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spendly.Desktop.Models;
using Spendly.Desktop.Services;

namespace Spendly.Desktop.ViewModels;

public partial class ReportsViewModel : ObservableObject
{
    private readonly MockDataService _data;

    // ── Filter ────────────────────────────────────────────────────────────────
    [ObservableProperty] private DateTime _from = new(DateTime.Now.Year, DateTime.Now.Month, 1);
    [ObservableProperty] private DateTime _to   = DateTime.Now;
    [ObservableProperty] private string _selectedType = "Sve";

    public List<string> TypeOptions { get; } = ["Sve", "Rashodi", "Prihodi"];
    public ObservableCollection<Transaction> Filtered { get; } = [];

    // ── Add form ──────────────────────────────────────────────────────────────
    [ObservableProperty] private bool    _isAddFormOpen;
    [ObservableProperty] private string  _newDescription = string.Empty;
    [ObservableProperty] private string  _newCategory    = string.Empty;
    [ObservableProperty] private decimal _newAmount;
    [ObservableProperty] private string  _newSelectedType  = "Rashod";
    [ObservableProperty] private string  _newSelectedScope = "Osobno";
    [ObservableProperty] private DateTime _newDate = DateTime.Now;
    [ObservableProperty] private string  _addError = string.Empty;

    public List<string> NewTypeOptions  { get; } = ["Rashod", "Prihod"];
    public List<string> NewScopeOptions { get; } = ["Osobno", "Obiteljski"];

    public ObservableCollection<string> Categories => _data.Categories;
    public string  CurrencySymbol => _data.CurrencySymbol;
    public decimal ExchangeRate   => _data.ExchangeRate;
    public bool    HasNoTransactions => Filtered.Count == 0;
    public bool    HasTransactions   => Filtered.Count > 0;
    public int     FilteredCount     => Filtered.Count;
    public decimal FilteredIncome   => Filtered.Where(t => t.IsIncome).Sum(t => t.Amount * _data.ExchangeRate);
    public decimal FilteredExpenses => Filtered.Where(t => !t.IsIncome).Sum(t => t.Amount * _data.ExchangeRate);

    public ReportsViewModel(MockDataService data)
    {
        _data = data;
        _data.PropertyChanged += (_, _) => { OnPropertyChanged(nameof(CurrencySymbol)); OnPropertyChanged(nameof(ExchangeRate)); OnPropertyChanged(nameof(FilteredIncome)); OnPropertyChanged(nameof(FilteredExpenses)); };
        Filtered.CollectionChanged += (_, _) => { OnPropertyChanged(nameof(HasNoTransactions)); OnPropertyChanged(nameof(HasTransactions)); OnPropertyChanged(nameof(FilteredCount)); OnPropertyChanged(nameof(FilteredIncome)); OnPropertyChanged(nameof(FilteredExpenses)); };
        ApplyFilters();
    }

    [RelayCommand]
    private void ToggleAddForm()
    {
        IsAddFormOpen   = !IsAddFormOpen;
        NewDescription  = string.Empty;
        NewCategory     = string.Empty;
        NewAmount       = 0;
        NewSelectedType  = "Rashod";
        NewSelectedScope = "Osobno";
        NewDate         = DateTime.Now;
        AddError        = string.Empty;
    }

    [RelayCommand]
    private void AddTransaction()
    {
        if (string.IsNullOrWhiteSpace(NewDescription)) { AddError = "Unesite opis transakcije."; return; }
        if (string.IsNullOrWhiteSpace(NewCategory))    { AddError = "Unesite kategoriju."; return; }
        if (NewAmount <= 0)                            { AddError = "Iznos mora biti veći od 0."; return; }

        _data.Transactions.Add(new Transaction
        {
            Id          = _data.Transactions.Count > 0 ? _data.Transactions.Max(t => t.Id) + 1 : 1,
            Description = NewDescription,
            Category    = NewCategory,
            Amount      = NewAmount,
            Type        = NewSelectedType == "Prihod" ? TransactionType.Income : TransactionType.Expense,
            Scope       = NewSelectedScope == "Obiteljski" ? TransactionScope.Family : TransactionScope.Personal,
            Date        = NewDate,
        });

        IsAddFormOpen = false;
        AddError      = string.Empty;
        ApplyFilters();
    }

    [RelayCommand]
    private void RequestDeleteTransaction(Transaction t)
    {
        foreach (var tx in _data.Transactions) tx.IsConfirmingDelete = false;
        t.IsConfirmingDelete = true;
    }

    [RelayCommand]
    private void ConfirmDeleteTransaction(Transaction t)
    {
        _data.Transactions.Remove(t);
        ApplyFilters();
    }

    [RelayCommand]
    private void CancelDeleteTransaction(Transaction t) => t.IsConfirmingDelete = false;

    [RelayCommand]
    private void ResetFilters()
    {
        From = new(DateTime.Now.Year, DateTime.Now.Month, 1);
        To = DateTime.Now;
        SelectedType = "Sve";
        ApplyFilters();
    }

    [RelayCommand]
    private void ApplyFilters()
    {
        var results = _data.Transactions.Where(t =>
            t.Date.Date >= From.Date &&
            t.Date.Date <= To.Date &&
            SelectedType switch
            {
                "Rashodi" => t.Type == TransactionType.Expense,
                "Prihodi" => t.Type == TransactionType.Income,
                _         => true,
            });

        Filtered.Clear();
        foreach (var t in results.OrderByDescending(t => t.Date))
            Filtered.Add(t);
    }
}
