using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Spendly.Desktop.Models;

namespace Spendly.Desktop.ViewModels;

public partial class FamilyViewModel : ObservableObject
{
    public ObservableCollection<FamilyMember> Members { get; } =
    [
        new() { Name="Ana Kovač",   Email="ana@kovac.hr",   Role="Vlasnik", AvatarColor="#3B82F6", TotalIncome=4250.00m, TotalSpent=1420.00m },
        new() { Name="Marko Kovač", Email="marko@kovac.hr", Role="Član",    AvatarColor="#22C55E", TotalIncome=3200.00m, TotalSpent=980.00m  },
        new() { Name="Lena Kovač",  Email="lena@kovac.hr",  Role="Član",    AvatarColor="#F59E0B", TotalIncome=0.00m,    TotalSpent=447.50m  },
    ];
}
