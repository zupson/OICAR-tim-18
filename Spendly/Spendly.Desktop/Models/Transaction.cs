using CommunityToolkit.Mvvm.ComponentModel;

namespace Spendly.Desktop.Models;

public partial class Transaction : ObservableObject
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public TransactionScope Scope { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotConfirmingDelete))]
    private bool _isConfirmingDelete;

    public bool IsNotConfirmingDelete => !IsConfirmingDelete;
    public bool IsIncome => Type == TransactionType.Income;
    public string FormattedAmount => IsIncome ? $"+{Amount:N2}" : $"-{Amount:N2}";
    public string ScopeLabel => Scope == TransactionScope.Family ? "Obiteljski" : "Osobno";
}

public enum TransactionType { Expense, Income }
public enum TransactionScope { Personal, Family }
