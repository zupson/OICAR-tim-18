using Spendly.Desktop.Models;
using Spendly.Desktop.Services;
using Spendly.Desktop.ViewModels;

namespace Spendly.Desktop.Tests.ViewModels;

public class BudgetViewModelTests
{
    private static DataCache Cache(params Transaction[] txns)
    {
        var c = new DataCache();
        foreach (var t in txns) c.Transactions.Add(t);
        return c;
    }

    private static Transaction Expense(decimal amount, string category, TransactionScope scope)
        => new() { Amount = amount, Type = TransactionType.Expense, Category = category, Scope = scope, Date = DateTime.Now };

    [Fact]
    public void BudgetRow_CategorySpending_GroupsAndSumsCorrectly()
    {
        var now = DateTime.Now;
        var budget = new Budget { Id = 1, Limit = 500m, ApiMonth = now.Month, ApiYear = now.Year };
        var cache = Cache(
            Expense(10m, "Hrana", TransactionScope.Personal),
            Expense(20m, "Hrana", TransactionScope.Personal),
            Expense(15m, "Transport", TransactionScope.Personal));
        var row = new BudgetRowViewModel(budget, cache);

        var hrana = row.CategorySpending.First(c => c.Category == "Hrana");
        Assert.Equal(30m, hrana.Amount);
    }
}
