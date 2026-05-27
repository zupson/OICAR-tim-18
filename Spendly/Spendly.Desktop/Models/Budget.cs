using CommunityToolkit.Mvvm.ComponentModel;

namespace Spendly.Desktop.Models;

public partial class Budget : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _category = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Remaining), nameof(RemainingAbsolute), nameof(IsOverBudget), nameof(RemainingLabel), nameof(ProgressPercent), nameof(AlertLevel))]
    private decimal _limit;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Remaining), nameof(RemainingAbsolute), nameof(IsOverBudget), nameof(RemainingLabel), nameof(ProgressPercent), nameof(AlertLevel))]
    private decimal _spent;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotEditing))]
    private bool _isEditing;

    [ObservableProperty] private decimal _editLimit;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotConfirmingDelete))]
    private bool _isConfirmingDelete;

    public int? ApiMonth        { get; set; }
    public int? ApiYear         { get; set; }
    public int? ApiUserGroupId  { get; set; }

    public bool IsNotConfirmingDelete => !IsConfirmingDelete;
    public bool IsNotEditing          => !IsEditing;
    public decimal Remaining      => Limit - Spent;
    public decimal RemainingAbsolute => Math.Abs(Remaining);
    public bool IsOverBudget      => Spent > Limit;
    public string RemainingLabel  => IsOverBudget ? "Prekoračeno" : "Preostalo";
    public double ProgressPercent => Limit > 0 ? Math.Min((double)(Spent / Limit) * 100, 100) : 0;
    public AlertLevel AlertLevel  => ProgressPercent >= 100 ? AlertLevel.Critical
                                   : ProgressPercent >= 80  ? AlertLevel.Warning
                                   : AlertLevel.Normal;
}

public enum AlertLevel { Normal, Warning, Critical }
