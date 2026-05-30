using CommunityToolkit.Mvvm.ComponentModel;

namespace Spendly.Desktop.Models;

public partial class FamilyMember : ObservableObject
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string AvatarColor { get; set; } = "#3B82F6";
    public decimal TotalIncome { get; set; }
    public decimal TotalSpent { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotConfirmingDelete))]
    private bool _isConfirmingDelete;

    public bool IsNotConfirmingDelete => !IsConfirmingDelete;
    public decimal Balance => TotalIncome - TotalSpent;
    public bool IsBalancePositive => Balance >= 0;
    public string AvatarInitials => Name.Length > 0
        ? string.Concat(Name.Split(' ').Take(2).Select(p => p[0].ToString().ToUpper()))
        : "?";
}
