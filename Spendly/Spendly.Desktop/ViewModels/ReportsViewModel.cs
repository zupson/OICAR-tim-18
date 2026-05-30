using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spendly.Desktop.Models;
using Spendly.Desktop.Services;

namespace Spendly.Desktop.ViewModels;

public partial class ReportsViewModel : ObservableObject
{
    private readonly DataCache _data;
    private readonly ApiService      _api;

    [ObservableProperty] private DateTime _from = new(DateTime.Now.Year, DateTime.Now.Month, 1);
    [ObservableProperty] private DateTime _to   = DateTime.Now;
    [ObservableProperty] private string _selectedType = "Sve";

    public List<string> TypeOptions { get; } = ["Sve", "Rashodi", "Prihodi"];
    public ObservableCollection<Transaction> Filtered { get; } = [];

    [ObservableProperty] private bool    _isAddFormOpen;
    [ObservableProperty] private string  _newDescription = string.Empty;
    [ObservableProperty] private string  _newCategory    = string.Empty;
    [ObservableProperty] private decimal _newAmount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AvailableCategories))]
    private string _newSelectedType  = "Rashod";

    [ObservableProperty] private string   _newSelectedScope = "Osobno";
    [ObservableProperty] private DateTime _newDate = DateTime.Now;
    [ObservableProperty] private string   _addError = string.Empty;

    private Transaction? _editingTransaction;
    public bool IsEditing => _editingTransaction is not null;
    public string FormTitle => IsEditing ? "Uredi transakciju" : "Nova transakcija";

    public List<string> NewTypeOptions  { get; } = ["Rashod", "Prihod"];
    public List<string> NewScopeOptions { get; } = ["Osobno", "Obiteljski"];

    public ObservableCollection<string> AvailableCategories
        => NewSelectedType == "Prihod" ? _data.RevenueTypeNames : _data.CostTypeNames;

    public string  CurrencySymbol => _data.CurrencySymbol;
    public decimal ExchangeRate   => _data.ExchangeRate;
    public bool    HasNoTransactions => Filtered.Count == 0;
    public bool    HasTransactions   => Filtered.Count > 0;
    public int     FilteredCount     => Filtered.Count;
    public decimal FilteredIncome   => Filtered.Where(t => t.IsIncome).Sum(t => t.Amount * _data.ExchangeRate);
    public decimal FilteredExpenses => Filtered.Where(t => !t.IsIncome).Sum(t => t.Amount * _data.ExchangeRate);

    private static string FriendlyError(Exception ex)
    {
        if (ex is HttpRequestException hre)
        {
            return hre.StatusCode switch
            {
                HttpStatusCode.BadRequest          => "Neispravan zahtjev. Provjerite unesene podatke.",
                HttpStatusCode.NotFound            => "Stavka nije pronađena.",
                HttpStatusCode.Forbidden           => "Nemate ovlasti za ovu radnju.",
                HttpStatusCode.Unauthorized        => "Nemate ovlasti za ovu radnju.",
                HttpStatusCode.InternalServerError => "Greška na poslužitelju. Pokušajte ponovo.",
                _                                  => "Neočekivana greška. Pokušajte ponovo."
            };
        }
        return "Neočekivana greška. Pokušajte ponovo.";
    }

    public ReportsViewModel(DataCache data, ApiService api)
    {
        _data = data;
        _api  = api;
        _data.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CurrencySymbol));
            OnPropertyChanged(nameof(ExchangeRate));
            OnPropertyChanged(nameof(FilteredIncome));
            OnPropertyChanged(nameof(FilteredExpenses));
        };
        _data.Transactions.CollectionChanged += (_, _) =>
        {
            ApplyFilters();
        };
        _data.CostTypeNames.CollectionChanged    += (_, _) => OnPropertyChanged(nameof(AvailableCategories));
        _data.RevenueTypeNames.CollectionChanged += (_, _) => OnPropertyChanged(nameof(AvailableCategories));
        Filtered.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasNoTransactions));
            OnPropertyChanged(nameof(HasTransactions));
            OnPropertyChanged(nameof(FilteredCount));
            OnPropertyChanged(nameof(FilteredIncome));
            OnPropertyChanged(nameof(FilteredExpenses));
        };
        ApplyFilters();
    }

    [RelayCommand]
    private void ToggleAddForm()
    {
        _editingTransaction = null;
        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(FormTitle));
        IsAddFormOpen    = !IsAddFormOpen;
        NewDescription   = string.Empty;
        NewCategory      = string.Empty;
        NewAmount        = 0;
        NewSelectedType  = "Rashod";
        NewSelectedScope = "Osobno";
        NewDate          = DateTime.Now;
        AddError         = string.Empty;
    }

    [RelayCommand]
    private void RequestEditTransaction(Transaction t)
    {
        foreach (var tx in _data.Transactions) tx.IsConfirmingDelete = false;
        _editingTransaction = t;
        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(FormTitle));
        NewDescription   = t.Description;
        NewSelectedType  = t.IsIncome ? "Prihod" : "Rashod";
        NewCategory      = t.Category;
        NewAmount        = t.Amount;
        NewSelectedScope = t.ScopeLabel;
        NewDate          = t.Date;
        AddError         = string.Empty;
        IsAddFormOpen    = true;
    }

    [RelayCommand]
    private async Task SaveEditTransaction()
    {
        if (_editingTransaction is null) return;
        if (string.IsNullOrWhiteSpace(NewDescription)) { AddError = "Unesite opis transakcije."; return; }
        if (string.IsNullOrWhiteSpace(NewCategory))    { AddError = "Odaberite kategoriju."; return; }
        if (NewAmount <= 0)                            { AddError = "Iznos mora biti veći od 0."; return; }

        bool isRevenue = _editingTransaction.IsApiRevenue;
        var option = _data.TypeOptions.FirstOrDefault(o => o.Name == NewCategory && o.IsRevenue == isRevenue)
                     ?? _data.TypeOptions.FirstOrDefault(o => o.IsRevenue == isRevenue);
        if (option is null) { AddError = "Nepoznata kategorija."; return; }

        try
        {
            if (isRevenue)
                await _api.PutAsync($"/api/Revenue/EditRevenueType/{_editingTransaction.Id}",
                    new { amount = NewAmount, transactionDate = NewDate, notes = NewDescription, currency = 0, revenueTypeId = option.TypeId });
            else
                await _api.PutAsync($"/api/Cost/EditCostType/{_editingTransaction.Id}",
                    new { amount = NewAmount, transactionDate = NewDate, notes = NewDescription, currency = 0, costTypeId = option.TypeId });

            _editingTransaction.Amount      = NewAmount;
            _editingTransaction.Description = NewDescription;
            _editingTransaction.Category    = NewCategory;
            _editingTransaction.Date        = NewDate;
            _editingTransaction.TypeId      = option.TypeId;

            _editingTransaction = null;
            OnPropertyChanged(nameof(IsEditing));
            OnPropertyChanged(nameof(FormTitle));
            IsAddFormOpen = false;
            AddError      = string.Empty;
            ApplyFilters();
        }
        catch (Exception ex)
        {
            AddError = FriendlyError(ex);
        }
    }

    [RelayCommand]
    private async Task AddTransaction()
    {
        if (string.IsNullOrWhiteSpace(NewDescription)) { AddError = "Unesite opis transakcije."; return; }
        if (string.IsNullOrWhiteSpace(NewCategory))    { AddError = "Odaberite kategoriju."; return; }
        if (NewAmount <= 0)                            { AddError = "Iznos mora biti veći od 0."; return; }

        bool isRevenue = NewSelectedType == "Prihod";
        var option = _data.TypeOptions.FirstOrDefault(o => o.Name == NewCategory && o.IsRevenue == isRevenue);
        if (option is null) { AddError = "Nepoznata kategorija."; return; }

        int groupId = NewSelectedScope == "Obiteljski" && _api.FamilyGroupId > 0
            ? _api.FamilyGroupId
            : _api.PersonalGroupId;

        try
        {
            if (isRevenue)
            {
                var created = await _api.PostAsync<object, ApiRevenue>(
                    $"/api/Revenue/CreateNewRevenue/{groupId}",
                    new { amount = NewAmount, transactionDate = NewDate, notes = NewDescription, currency = 0, revenueTypeId = option.TypeId });

                if (created is not null)
                    _data.Transactions.Add(new Transaction
                    {
                        Id           = created.Id,
                        TypeId       = option.TypeId,
                        Description  = NewDescription,
                        Category     = NewCategory,
                        Amount       = NewAmount,
                        Date         = NewDate,
                        Type         = TransactionType.Income,
                        Scope        = NewSelectedScope == "Obiteljski" ? TransactionScope.Family : TransactionScope.Personal,
                        IsApiRevenue = true,
                    });
            }
            else
            {
                var created = await _api.PostAsync<object, ApiCost>(
                    $"/api/Cost/CreateNewCost/{groupId}",
                    new { amount = NewAmount, transactionDate = NewDate, notes = NewDescription, currency = 0, costTypeId = option.TypeId });

                if (created is not null)
                    _data.Transactions.Add(new Transaction
                    {
                        Id           = created.Id,
                        TypeId       = option.TypeId,
                        Description  = NewDescription,
                        Category     = NewCategory,
                        Amount       = NewAmount,
                        Date         = NewDate,
                        Type         = TransactionType.Expense,
                        Scope        = NewSelectedScope == "Obiteljski" ? TransactionScope.Family : TransactionScope.Personal,
                        IsApiRevenue = false,
                    });
            }

            IsAddFormOpen = false;
            AddError      = string.Empty;
        }
        catch (Exception ex)
        {
            AddError = FriendlyError(ex);
        }
    }

    [RelayCommand]
    private void RequestDeleteTransaction(Transaction t)
    {
        foreach (var tx in _data.Transactions) tx.IsConfirmingDelete = false;
        t.IsConfirmingDelete = true;
    }

    [RelayCommand]
    private async Task ConfirmDeleteTransaction(Transaction t)
    {
        try
        {
            string path = t.IsApiRevenue
                ? $"/api/Revenue/DeleteRevenue/{t.Id}"
                : $"/api/Cost/{t.Id}";
            await _api.DeleteAsync(path);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Delete failed: {ex.Message}");
        }

        _data.Transactions.Remove(t);
        ApplyFilters();
    }

    [RelayCommand]
    private void CancelDeleteTransaction(Transaction t) => t.IsConfirmingDelete = false;

    [RelayCommand]
    private void ResetFilters()
    {
        From         = new(DateTime.Now.Year, DateTime.Now.Month, 1);
        To           = DateTime.Now;
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
