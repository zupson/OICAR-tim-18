using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spendly.Desktop.Models;
using Spendly.Desktop.Services;

namespace Spendly.Desktop.ViewModels;

public partial class BudgetViewModel : ObservableObject
{
    private readonly MockDataService _data;

    public ObservableCollection<Budget> Budgets => _data.Budgets;

    [ObservableProperty] private bool _isAddFormOpen;
    [ObservableProperty] private string _newName = string.Empty;
    [ObservableProperty] private string _newCategory = string.Empty;
    [ObservableProperty] private decimal _newLimit;
    [ObservableProperty] private string _addError = string.Empty;

    public string  CurrencySymbol => _data.CurrencySymbol;
    public decimal ExchangeRate   => _data.ExchangeRate;

    public BudgetViewModel(MockDataService data)
    {
        _data = data;
        _data.PropertyChanged += (_, _) => { OnPropertyChanged(nameof(CurrencySymbol)); OnPropertyChanged(nameof(ExchangeRate)); };
    }

    [RelayCommand]
    private void ToggleAddForm()
    {
        IsAddFormOpen = !IsAddFormOpen;
        NewName = string.Empty;
        NewCategory = string.Empty;
        NewLimit = 0;
        AddError = string.Empty;
    }

    [RelayCommand]
    private void AddBudget()
    {
        if (string.IsNullOrWhiteSpace(NewName))    { AddError = "Unesite naziv proračuna."; return; }
        if (string.IsNullOrWhiteSpace(NewCategory)) { AddError = "Unesite kategoriju."; return; }
        if (NewLimit <= 0)                          { AddError = "Limit mora biti veći od 0."; return; }

        _data.Budgets.Add(new Budget
        {
            Id       = _data.Budgets.Count > 0 ? _data.Budgets.Max(b => b.Id) + 1 : 1,
            Name     = NewName,
            Category = NewCategory,
            Limit    = NewLimit,
            Spent    = 0,
        });

        IsAddFormOpen = false;
        AddError = string.Empty;
    }

    [RelayCommand]
    private void DeleteBudget(Budget budget) => _data.Budgets.Remove(budget);

    [RelayCommand]
    private void StartEdit(Budget budget)
    {
        foreach (var b in _data.Budgets) b.IsEditing = false;
        budget.EditLimit = budget.Limit;
        budget.IsEditing = true;
    }

    [RelayCommand]
    private void SaveEdit(Budget budget)
    {
        if (budget.EditLimit > 0)
            budget.Limit = budget.EditLimit;
        budget.IsEditing = false;
    }

    [RelayCommand]
    private void CancelEdit(Budget budget) => budget.IsEditing = false;
}
