using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Spendly.Desktop.Models;
using Spendly.Desktop.Services;

namespace Spendly.Desktop.ViewModels;

public class NotificationItem
{
    public string Title   { get; init; } = string.Empty;
    public string Detail  { get; init; } = string.Empty;
    public string Level   { get; init; } = "info";
    public bool   IsCritical => Level == "critical";
    public bool   IsWarning  => Level == "warning";
    public bool   IsInfo     => Level == "info";
}

public partial class NotificationsViewModel : ObservableObject
{
    private readonly DataCache       _data;
    private readonly SettingsService _settingsService;

    public ObservableCollection<NotificationItem> Items { get; } = [];
    public bool HasNoItems => Items.Count == 0;

    public NotificationsViewModel(DataCache data, SettingsService settingsService)
    {
        _data            = data;
        _settingsService = settingsService;
        data.Budgets.CollectionChanged      += (_, _) => Rebuild();
        data.Transactions.CollectionChanged += (_, _) => Rebuild();
        Rebuild();
    }

    public void Rebuild()
    {
        Items.Clear();
        var settings = _settingsService.Load();

        foreach (var b in _data.Budgets)
        {
            var pct = b.Limit > 0 ? (double)b.Spent / (double)b.Limit * 100 : 0;
            if (pct >= 100 && settings.BudgetCriticalAlerts)
                Items.Add(new NotificationItem
                {
                    Title  = $"Proračun prekoračen — {b.Name}",
                    Detail = $"Potrošeno {pct:F0}% proračuna ({b.Spent:F2} € od {b.Limit:F2} €).",
                    Level  = "critical"
                });
            else if (pct >= 80 && settings.BudgetWarningAlerts)
                Items.Add(new NotificationItem
                {
                    Title  = $"Upozorenje — {b.Name}",
                    Detail = $"Potrošeno {pct:F0}% proračuna ({b.Spent:F2} € od {b.Limit:F2} €).",
                    Level  = "warning"
                });
        }

        if (!_data.Budgets.Any())
            Items.Add(new NotificationItem
            {
                Title  = "Nema postavljenih proračuna",
                Detail = "Dodajte proračun u kartici Proračun kako biste pratili potrošnju.",
                Level  = "info"
            });

        if (!_data.Transactions.Any())
            Items.Add(new NotificationItem
            {
                Title  = "Dobrodošli u Spendly!",
                Detail = "Dodajte prvu transakciju u kartici Transakcije kako biste počeli pratiti financije.",
                Level  = "info"
            });

        OnPropertyChanged(nameof(HasNoItems));
    }
}
