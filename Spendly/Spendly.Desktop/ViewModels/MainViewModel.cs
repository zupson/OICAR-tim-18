using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Spendly.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDashboardActive), nameof(IsBudgetActive), nameof(IsReportsActive),
        nameof(IsFamilyActive), nameof(IsSettingsActive), nameof(IsObavijestActive),
        nameof(IsCategoriesActive), nameof(IsReportsChartActive))]
    private ObservableObject _currentPage;
    [ObservableProperty] private string _loggedInUser = string.Empty;

    public bool IsDashboardActive    => CurrentPage is DashboardViewModel;
    public bool IsBudgetActive       => CurrentPage is BudgetViewModel;
    public bool IsReportsActive      => CurrentPage is ReportsViewModel;
    public bool IsFamilyActive       => CurrentPage is FamilyViewModel;
    public bool IsSettingsActive     => CurrentPage is SettingsViewModel;
    public bool IsObavijestActive    => CurrentPage is NotificationsViewModel;
    public bool IsCategoriesActive   => CurrentPage is CategoriesViewModel;
    public bool IsReportsChartActive => CurrentPage is ReportsChartViewModel;

    public MainViewModel(
        DashboardViewModel dashboard,
        BudgetViewModel budget,
        ReportsViewModel reports,
        FamilyViewModel family,
        SettingsViewModel settings,
        CategoriesViewModel categories,
        ReportsChartViewModel reportsChart,
        NotificationsViewModel notifications)
    {
        Dashboard     = dashboard;
        Budget        = budget;
        Reports       = reports;
        Family        = family;
        Settings      = settings;
        Categories    = categories;
        ReportsChart  = reportsChart;
        Notifications = notifications;
        _currentPage  = dashboard;
    }

    public DashboardViewModel     Dashboard     { get; }
    public BudgetViewModel        Budget        { get; }
    public ReportsViewModel       Reports       { get; }
    public FamilyViewModel        Family        { get; }
    public SettingsViewModel      Settings      { get; }
    public CategoriesViewModel    Categories    { get; }
    public ReportsChartViewModel  ReportsChart  { get; }
    public NotificationsViewModel Notifications { get; }

    [RelayCommand] private void NavigateDashboard()    => CurrentPage = Dashboard;
    [RelayCommand] private void NavigateBudget()       => CurrentPage = Budget;
    [RelayCommand] private void NavigateReports()      => CurrentPage = Reports;
    [RelayCommand] private void NavigateFamily()       => CurrentPage = Family;
    [RelayCommand] private void NavigateSettings()     => CurrentPage = Settings;
    [RelayCommand] private void NavigateObavijest()    => CurrentPage = Notifications;
    [RelayCommand] private void NavigateCategories()   => CurrentPage = Categories;
    [RelayCommand] private void NavigateReportsChart() => CurrentPage = ReportsChart;

    public event Action? LogoutRequested;

    [RelayCommand]
    private void Logout() => LogoutRequested?.Invoke();
}
