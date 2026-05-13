using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using Spendly.Desktop.Models;
using Spendly.Desktop.Services;

namespace Spendly.Desktop.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly MockDataService _data;

    public string  CurrencySymbol => _data.CurrencySymbol;
    public decimal ExchangeRate   => _data.ExchangeRate;

    public DashboardViewModel(MockDataService data)
    {
        _data = data;
        _data.Transactions.CollectionChanged += OnTransactionsChanged;
        _data.PropertyChanged += (_, _) => { OnPropertyChanged(nameof(CurrencySymbol)); OnPropertyChanged(nameof(ExchangeRate)); };
    }

    public decimal TotalIncome    => _data.Transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
    public decimal TotalExpenses  => _data.Transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
    public decimal Balance        => TotalIncome - TotalExpenses;
    public decimal SharedExpenses => _data.Transactions.Where(t => t.Type == TransactionType.Expense && t.Scope == TransactionScope.Family).Sum(t => t.Amount);

    public IEnumerable<Transaction> RecentTransactions => _data.Transactions.OrderByDescending(t => t.Date).Take(5);

    private void OnTransactionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(TotalIncome));
        OnPropertyChanged(nameof(TotalExpenses));
        OnPropertyChanged(nameof(Balance));
        OnPropertyChanged(nameof(SharedExpenses));
        OnPropertyChanged(nameof(RecentTransactions));
    }
}
