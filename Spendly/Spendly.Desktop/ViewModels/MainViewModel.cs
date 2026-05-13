using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Spendly.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDashboardActive), nameof(IsBudgetActive), nameof(IsReportsActive), nameof(IsFamilyActive), nameof(IsSettingsActive))]
    private ObservableObject _currentPage;
    [ObservableProperty] private string _loggedInUser = string.Empty;

    public bool IsDashboardActive => CurrentPage is DashboardViewModel;
    public bool IsBudgetActive    => CurrentPage is BudgetViewModel;
    public bool IsReportsActive   => CurrentPage is ReportsViewModel;
    public bool IsFamilyActive    => CurrentPage is FamilyViewModel;
    public bool IsSettingsActive  => CurrentPage is SettingsViewModel;

    public MainViewModel(
        DashboardViewModel dashboard,
        BudgetViewModel budget,
        ReportsViewModel reports,
        FamilyViewModel family,
        SettingsViewModel settings)
    {
        Dashboard = dashboard;
        Budget = budget;
        Reports = reports;
        Family = family;
        Settings = settings;
        _currentPage = dashboard;
    }

    public DashboardViewModel Dashboard { get; }
    public BudgetViewModel Budget { get; }
    public ReportsViewModel Reports { get; }
    public FamilyViewModel Family { get; }
    public SettingsViewModel Settings { get; }

    [RelayCommand] private void NavigateDashboard() => CurrentPage = Dashboard;
    [RelayCommand] private void NavigateBudget()    => CurrentPage = Budget;
    [RelayCommand] private void NavigateReports()   => CurrentPage = Reports;
    [RelayCommand] private void NavigateFamily()    => CurrentPage = Family;
    [RelayCommand] private void NavigateSettings()  => CurrentPage = Settings;

    public event Action? LogoutRequested;

    [RelayCommand]
    private void Logout() => LogoutRequested?.Invoke();
}
