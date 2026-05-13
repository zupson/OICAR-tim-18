using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spendly.Desktop.Models;

namespace Spendly.Desktop.ViewModels;

public partial class ReportsViewModel : ObservableObject
{
    private readonly List<Transaction> _all =
    [
        new() { Id=1,  Amount=42.50m,   Description="Konzum — namirnice",   Category="Hrana",     Date=DateTime.Now.AddDays(-1),  Type=TransactionType.Expense, Scope=TransactionScope.Family  },
        new() { Id=2,  Amount=4250.00m, Description="Plaća — Travanj",      Category="Plaća",     Date=DateTime.Now.AddDays(-2),  Type=TransactionType.Income,  Scope=TransactionScope.Personal},
        new() { Id=3,  Amount=89.90m,   Description="Struja — HEP",         Category="Režije",    Date=DateTime.Now.AddDays(-3),  Type=TransactionType.Expense, Scope=TransactionScope.Family  },
        new() { Id=4,  Amount=25.00m,   Description="Autobusna karta",       Category="Prijevoz",  Date=DateTime.Now.AddDays(-5),  Type=TransactionType.Expense, Scope=TransactionScope.Personal},
        new() { Id=5,  Amount=1200.00m, Description="Freelance projekt",     Category="Freelance", Date=DateTime.Now.AddDays(-7),  Type=TransactionType.Income,  Scope=TransactionScope.Personal},
        new() { Id=6,  Amount=15.99m,   Description="Netflix pretplata",     Category="Zabava",    Date=DateTime.Now.AddDays(-8),  Type=TransactionType.Expense, Scope=TransactionScope.Family  },
        new() { Id=7,  Amount=65.00m,   Description="Ljekarna",              Category="Zdravlje",  Date=DateTime.Now.AddDays(-10), Type=TransactionType.Expense, Scope=TransactionScope.Personal},
        new() { Id=8,  Amount=320.00m,  Description="Kirija — stan",         Category="Stanovanje",Date=DateTime.Now.AddDays(-12), Type=TransactionType.Expense, Scope=TransactionScope.Family  },
        new() { Id=9,  Amount=55.00m,   Description="Gorivo",               Category="Prijevoz",  Date=DateTime.Now.AddDays(-14), Type=TransactionType.Expense, Scope=TransactionScope.Personal},
        new() { Id=10, Amount=800.00m,  Description="Bonus",                 Category="Plaća",     Date=DateTime.Now.AddDays(-15), Type=TransactionType.Income,  Scope=TransactionScope.Personal},
    ];

    [ObservableProperty] private DateTime _from = new(DateTime.Now.Year, DateTime.Now.Month, 1);
    [ObservableProperty] private DateTime _to   = DateTime.Now;
    [ObservableProperty] private string _selectedType = "Sve";

    public List<string> TypeOptions { get; } = ["Sve", "Rashodi", "Prihodi"];
    public ObservableCollection<Transaction> Filtered { get; } = [];

    public ReportsViewModel() => ApplyFilters();

    [RelayCommand]
    private void ApplyFilters()
    {
        var results = _all.Where(t =>
            t.Date.Date >= From.Date &&
            t.Date.Date <= To.Date &&
            SelectedType switch
            {
                "Rashodi" => t.Type == TransactionType.Expense,
                "Prihodi" => t.Type == TransactionType.Income,
                _         => true,
            });

        Filtered.Clear();
        foreach (var t in results.OrderByDescending(t => t.Date))
            Filtered.Add(t);
    }
}
