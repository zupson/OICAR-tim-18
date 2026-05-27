using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using Spendly.Desktop.Models;
using Spendly.Desktop.Services;

namespace Spendly.Desktop.ViewModels;

public record CategoryBar(string Category, decimal Amount, double BarPercent);

public partial class DashboardViewModel : ObservableObject
{
    private readonly DataCache _data;

    public string  CurrencySymbol => _data.CurrencySymbol;
    public decimal ExchangeRate   => _data.ExchangeRate;

    public DashboardViewModel(DataCache data)
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
    public bool HasNoRecentTransactions => !_data.Transactions.Any();

    public IEnumerable<CategoryBar> TopCategories
    {
        get
        {
            var groups = _data.Transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.Category)
                .Select(g => (Category: g.Key, Total: g.Sum(t => t.Amount)))
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToList();

            if (groups.Count == 0) return [];

            var max = (double)groups.Max(x => x.Total);
            return groups.Select(x => new CategoryBar(x.Category, x.Total, max > 0 ? (double)x.Total / max * 100 : 0));
        }
    }

    public bool HasNoCategories => !_data.Transactions.Any(t => t.Type == TransactionType.Expense);

    private void OnTransactionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(TotalIncome));
        OnPropertyChanged(nameof(TotalExpenses));
        OnPropertyChanged(nameof(Balance));
        OnPropertyChanged(nameof(SharedExpenses));
        OnPropertyChanged(nameof(RecentTransactions));
        OnPropertyChanged(nameof(HasNoRecentTransactions));
        OnPropertyChanged(nameof(TopCategories));
        OnPropertyChanged(nameof(HasNoCategories));
    }
}
