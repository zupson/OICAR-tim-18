using CommunityToolkit.Mvvm.ComponentModel;

namespace Spendly.Desktop.Models;

public partial class FamilyMember : ObservableObject
{
    public const int RoleClan    = 1;
    public const int RoleUrednik = 2;
    public const int RoleVlasnik = 3;

    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AvatarColor { get; set; } = "#3B82F6";
    public decimal TotalIncome { get; set; }
    public decimal TotalSpent { get; set; }

    public int  UserId       { get; set; }
    public int  UserGroupId  { get; set; }
    public bool IsCurrentUser { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Role), nameof(IsOwner))]
    private int _roleId = RoleClan;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanBeRemoved))]
    private bool _canManage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotConfirmingDelete))]
    private bool _isConfirmingDelete;

    public bool IsNotConfirmingDelete => !IsConfirmingDelete;

    public bool IsOwner => RoleId == RoleVlasnik;

    public string Role => RoleId switch
    {
        RoleVlasnik => "Vlasnik",
        RoleUrednik => "Urednik",
        _           => "Član",
    };

    // Vlasnik može ukloniti druge članove (ne sebe i ne vlasnika).
    public bool CanBeRemoved => CanManage;

    public decimal Balance => TotalIncome - TotalSpent;
    public bool IsBalancePositive => Balance >= 0;
    public string AvatarInitials => Name.Length > 0
        ? string.Concat(Name.Split(' ').Take(2).Select(p => p[0].ToString().ToUpper()))
        : "?";
}
