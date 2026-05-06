using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Spendly.Desktop.ViewModels;
using Spendly.Desktop.Views;

namespace Spendly.Desktop;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var sc = new ServiceCollection();

        sc.AddSingleton<DashboardViewModel>();
        sc.AddSingleton<BudgetViewModel>();
        sc.AddSingleton<ReportsViewModel>();
        sc.AddSingleton<FamilyViewModel>();
        sc.AddSingleton<SettingsViewModel>();
        sc.AddSingleton<MainViewModel>();
        sc.AddTransient<LoginViewModel>();

        sc.AddTransient<LoginWindow>();
        sc.AddSingleton<MainWindow>();

        Services = sc.BuildServiceProvider();

        Services.GetRequiredService<LoginWindow>().Show();
    }
}
