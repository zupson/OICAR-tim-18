using Spendly.Desktop.Models;
using Spendly.Desktop.Services;
using Spendly.Desktop.ViewModels;

namespace Spendly.Desktop.Tests.ViewModels;

public class DashboardViewModelTests
{
    private static DataCache Cache(params Transaction[] txns)
    {
        var c = new DataCache();
        foreach (var t in txns) c.Transactions.Add(t);
        return c;
    }

    private static Transaction Expense(decimal amount, string category = "Hrana")
        => new() { Amount = amount, Type = TransactionType.Expense, Category = category, Scope = TransactionScope.Personal, Date = DateTime.Now };

    private static Transaction Income(decimal amount)
        => new() { Amount = amount, Type = TransactionType.Income, Scope = TransactionScope.Personal, Date = DateTime.Now };

    [Fact]
    public void TotalIncome_SumsIncomeIgnoresExpenses()
    {
        var vm = new DashboardViewModel(Cache(Income(100m), Income(50m), Expense(30m)));
        Assert.Equal(150m, vm.TotalIncome);
    }

    [Fact]
    public void Balance_IsIncomeMinusExpenses()
    {
        var vm = new DashboardViewModel(Cache(Income(300m), Expense(80m)));
        Assert.Equal(220m, vm.Balance);
    }
}
