using CommunityToolkit.Mvvm.ComponentModel;

namespace Spendly.Desktop.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    public decimal TotalIncome    { get; } = 4_250.00m;
    public decimal TotalExpenses  { get; } = 2_847.50m;
    public decimal Balance        => TotalIncome - TotalExpenses;
    public decimal SharedExpenses { get; } = 1_120.00m;
}
