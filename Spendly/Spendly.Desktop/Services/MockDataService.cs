using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Spendly.Desktop.Models;

namespace Spendly.Desktop.Services;

public partial class MockDataService : ObservableObject
{
    [ObservableProperty] private string _currencySymbol = "€";
    [ObservableProperty] private decimal _exchangeRate = 1m;
    public ObservableCollection<Transaction> Transactions { get; } =
    [
        new() { Id=1,  Amount=42.50m,   Description="Konzum — namirnice",  Category="Hrana",      Date=DateTime.Now.AddDays(-1),  Type=TransactionType.Expense, Scope=TransactionScope.Family   },
        new() { Id=2,  Amount=4250.00m, Description="Plaća — Travanj",     Category="Plaća",      Date=DateTime.Now.AddDays(-2),  Type=TransactionType.Income,  Scope=TransactionScope.Personal },
        new() { Id=3,  Amount=89.90m,   Description="Struja — HEP",        Category="Režije",     Date=DateTime.Now.AddDays(-3),  Type=TransactionType.Expense, Scope=TransactionScope.Family   },
        new() { Id=4,  Amount=25.00m,   Description="Autobusna karta",      Category="Prijevoz",   Date=DateTime.Now.AddDays(-5),  Type=TransactionType.Expense, Scope=TransactionScope.Personal },
        new() { Id=5,  Amount=1200.00m, Description="Freelance projekt",    Category="Freelance",  Date=DateTime.Now.AddDays(-7),  Type=TransactionType.Income,  Scope=TransactionScope.Personal },
        new() { Id=6,  Amount=15.99m,   Description="Netflix pretplata",    Category="Zabava",     Date=DateTime.Now.AddDays(-8),  Type=TransactionType.Expense, Scope=TransactionScope.Family   },
        new() { Id=7,  Amount=65.00m,   Description="Ljekarna",             Category="Zdravlje",   Date=DateTime.Now.AddDays(-10), Type=TransactionType.Expense, Scope=TransactionScope.Personal },
        new() { Id=8,  Amount=320.00m,  Description="Kirija — stan",        Category="Stanovanje", Date=DateTime.Now.AddDays(-12), Type=TransactionType.Expense, Scope=TransactionScope.Family   },
        new() { Id=9,  Amount=55.00m,   Description="Gorivo",              Category="Prijevoz",   Date=DateTime.Now.AddDays(-14), Type=TransactionType.Expense, Scope=TransactionScope.Personal },
        new() { Id=10, Amount=800.00m,  Description="Bonus",                Category="Plaća",      Date=DateTime.Now.AddDays(-15), Type=TransactionType.Income,  Scope=TransactionScope.Personal },
    ];

    public ObservableCollection<Budget> Budgets { get; } =
    [
        new() { Id=1, Name="Hrana & Namirnice", Category="Hrana",     Limit=800, Spent=680 },
        new() { Id=2, Name="Prijevoz",          Category="Prijevoz",  Limit=300, Spent=245 },
        new() { Id=3, Name="Zabava",            Category="Zabava",    Limit=200, Spent=210 },
        new() { Id=4, Name="Zdravlje",          Category="Zdravlje",  Limit=400, Spent=120 },
        new() { Id=5, Name="Režije",            Category="Režije",    Limit=500, Spent=390 },
    ];

    public ObservableCollection<FamilyMember> FamilyMembers { get; } =
    [
        new() { Name="Ana Kovač",   Email="ana@kovac.hr",   Role="Vlasnik", AvatarColor="#3B82F6", TotalIncome=4250.00m, TotalSpent=1420.00m },
        new() { Name="Marko Kovač", Email="marko@kovac.hr", Role="Član",    AvatarColor="#22C55E", TotalIncome=3200.00m, TotalSpent=980.00m  },
        new() { Name="Lena Kovač",  Email="lena@kovac.hr",  Role="Član",    AvatarColor="#F59E0B", TotalIncome=0.00m,    TotalSpent=447.50m  },
    ];

    private int _nextTransactionId = 11;
    public int NextTransactionId() => _nextTransactionId++;
}
