using Spendly.Desktop.Models;
using Spendly.Desktop.Services;
using Spendly.Desktop.ViewModels;

namespace Spendly.Desktop.Tests.ViewModels;

public class ReportsViewModelTests
{
    private static DataCache Cache(params Transaction[] txns)
    {
        var c = new DataCache();
        foreach (var t in txns) c.Transactions.Add(t);
        return c;
    }

    private static Transaction Expense(decimal amount)
        => new() { Amount = amount, Type = TransactionType.Expense, Category = "Hrana", Scope = TransactionScope.Personal, Date = DateTime.Now };

    private static Transaction Income(decimal amount)
        => new() { Amount = amount, Type = TransactionType.Income, Category = "Plaća", Scope = TransactionScope.Personal, Date = DateTime.Now };

    [Fact]
    public void ApplyFilters_RashodiType_ReturnsOnlyExpenses()
    {
        var vm = new ReportsViewModel(Cache(Expense(10m), Expense(20m), Income(100m)), new ApiService());
        vm.SelectedType = "Rashodi";
        vm.ApplyFiltersCommand.Execute(null);

        Assert.Equal(2, vm.Filtered.Count);
        Assert.All(vm.Filtered, t => Assert.Equal(TransactionType.Expense, t.Type));
    }

    [Fact]
    public async Task AddTransaction_EmptyDescription_SetsAddError()
    {
        var vm = new ReportsViewModel(new DataCache(), new ApiService());
        vm.NewDescription   = "";
        vm.NewCategory      = "Hrana";
        vm.NewAmount        = 50m;
        vm.NewSelectedType  = "Rashod";
        vm.NewSelectedScope = "Osobno";

        await vm.AddTransactionCommand.ExecuteAsync(null);

        Assert.False(string.IsNullOrEmpty(vm.AddError));
    }
}
