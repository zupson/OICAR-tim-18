using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Spendly.Desktop.Models;

namespace Spendly.Desktop.Services;

public partial class DataCache : ObservableObject
{
    [ObservableProperty] private string _currencySymbol = "€";
    [ObservableProperty] private decimal _exchangeRate = 1m;

    public ObservableCollection<Transaction> Transactions { get; } = [];
    public ObservableCollection<Budget>      Budgets      { get; } = [];
    public ObservableCollection<FamilyMember> FamilyMembers { get; } = [];
    public ObservableCollection<string> CostTypeNames    { get; } = [];
    public ObservableCollection<string> RevenueTypeNames { get; } = [];
    public List<ApiTypeOption> TypeOptions { get; } = [];

    public DataCache()
    {
        Transactions.CollectionChanged += (_, _) => RecalculateBudgetSpent();
        Budgets.CollectionChanged      += (_, _) => { /* categories refreshed after full load */ };
    }

    public int NextTransactionId() => Transactions.Count > 0 ? Transactions.Max(t => t.Id) + 1 : 1;

    public void Reset()
    {
        Transactions.Clear();
        Budgets.Clear();
        FamilyMembers.Clear();
        CostTypeNames.Clear();
        RevenueTypeNames.Clear();
        TypeOptions.Clear();
        CurrencySymbol = "€";
        ExchangeRate   = 1m;
    }

    private void RecalculateBudgetSpent()
    {
        foreach (var budget in Budgets)
        {
            if (budget.ApiMonth.HasValue)
            {
                budget.Spent = Transactions
                    .Where(t => t.Type  == TransactionType.Expense &&
                                t.Scope == TransactionScope.Personal &&
                                t.Date.Month == budget.ApiMonth &&
                                t.Date.Year  == budget.ApiYear)
                    .Sum(t => t.Amount);
            }
            else
            {
                budget.Spent = Transactions
                    .Where(t => t.Type  == TransactionType.Expense &&
                                t.Scope == TransactionScope.Personal &&
                                string.Equals(t.Category, budget.Category, StringComparison.OrdinalIgnoreCase))
                    .Sum(t => t.Amount);
            }
        }
    }

    private static readonly string[] CroMonthNames =
        ["Siječanj", "Veljača", "Ožujak", "Travanj", "Svibanj", "Lipanj",
         "Srpanj", "Kolovoz", "Rujan", "Listopad", "Studeni", "Prosinac"];

    public static string FormatBudgetName(int year, int month)
        => $"{CroMonthNames[month - 1]} {year}";

    public async Task ReloadTypesAsync(ApiService api)
    {
        var costTypes    = api.PersonalGroupId > 0
            ? await api.GetAsync<ApiCostType[]>($"/api/CostType/GetAllCostTypesByGroup?groupId={api.PersonalGroupId}") ?? []
            : Array.Empty<ApiCostType>();
        var revenueTypes = api.PersonalGroupId > 0
            ? await api.GetAsync<ApiRevenueType[]>($"/api/RevenueType/GetAllRevenueTypesByGroup?groupId={api.PersonalGroupId}") ?? []
            : Array.Empty<ApiRevenueType>();

        TypeOptions.Clear();
        foreach (var ct in costTypes.Where(c => c.Name?.StartsWith("__deleted_") != true).DistinctBy(c => c.Name))
            TypeOptions.Add(new ApiTypeOption(ct.Name ?? "Ostalo", ct.Id, false, ct.GroupId));
        foreach (var rt in revenueTypes.Where(r => r.Name?.StartsWith("__deleted_") != true).DistinctBy(r => r.Name))
            TypeOptions.Add(new ApiTypeOption(rt.Name ?? "Ostalo", rt.Id, true, rt.GroupId));

        CostTypeNames.Clear();
        foreach (var ct in costTypes.Where(ct => ct.Name is not null && ct.Name.StartsWith("__deleted_") != true).DistinctBy(ct => ct.Name))
            CostTypeNames.Add(ct.Name!);

        RevenueTypeNames.Clear();
        foreach (var rt in revenueTypes.Where(rt => rt.Name is not null && rt.Name.StartsWith("__deleted_") != true).DistinctBy(rt => rt.Name))
            RevenueTypeNames.Add(rt.Name!);
    }

    public async Task LoadFromApiAsync(ApiService api)
    {
        var costs    = await api.GetAsync<ApiCost[]>("/api/Cost/GetAllCosts")         ?? [];
        var revenues = await api.GetAsync<ApiRevenue[]>("/api/Revenue/GetAllRevenues") ?? [];
        var budgets  = await api.GetAsync<ApiBudget[]>("/api/Budget/GetAllBudgets")    ?? [];
        var userGroups     = await api.GetAsync<ApiUserGroup[]>("/api/UserGroup/GetAllUserGroups") ?? [];
        var personalGroupId = userGroups.FirstOrDefault(g => g.IsPersonal)?.GroupId ?? api.PersonalGroupId;

        var costTypes    = api.PersonalGroupId > 0
            ? await api.GetAsync<ApiCostType[]>($"/api/CostType/GetAllCostTypesByGroup?groupId={api.PersonalGroupId}") ?? []
            : Array.Empty<ApiCostType>();
        var revenueTypes = api.PersonalGroupId > 0
            ? await api.GetAsync<ApiRevenueType[]>($"/api/RevenueType/GetAllRevenueTypesByGroup?groupId={api.PersonalGroupId}") ?? []
            : Array.Empty<ApiRevenueType>();

        TypeOptions.Clear();
        foreach (var ct in costTypes.Where(c => c.Name?.StartsWith("__deleted_") != true).DistinctBy(c => c.Name))
            TypeOptions.Add(new ApiTypeOption(ct.Name ?? "Ostalo", ct.Id, false, ct.GroupId));
        foreach (var rt in revenueTypes.Where(r => r.Name?.StartsWith("__deleted_") != true).DistinctBy(r => r.Name))
            TypeOptions.Add(new ApiTypeOption(rt.Name ?? "Ostalo", rt.Id, true, rt.GroupId));

        CostTypeNames.Clear();
        foreach (var ct in costTypes.Where(ct => ct.Name is not null && ct.Name.StartsWith("__deleted_") != true).DistinctBy(ct => ct.Name))
            CostTypeNames.Add(ct.Name!);

        RevenueTypeNames.Clear();
        foreach (var rt in revenueTypes.Where(rt => rt.Name is not null && rt.Name.StartsWith("__deleted_") != true).DistinctBy(rt => rt.Name))
            RevenueTypeNames.Add(rt.Name!);

        Budgets.Clear();
        Transactions.Clear();
        foreach (var c in costs)
            Transactions.Add(new Transaction
            {
                Id           = c.Id,
                TypeId       = c.CostTypeId,
                Description  = string.IsNullOrWhiteSpace(c.Notes) ? (c.CostTypeName ?? "Rashod") : c.Notes,
                Category     = c.CostTypeName ?? "Ostalo",
                Amount       = c.Amount,
                Date         = c.TransactionDate,
                Type         = TransactionType.Expense,
                Scope        = c.GroupId == personalGroupId ? TransactionScope.Personal : TransactionScope.Family,
                IsApiRevenue = false,
            });
        foreach (var r in revenues)
            Transactions.Add(new Transaction
            {
                Id           = r.Id,
                TypeId       = r.RevenueTypeId,
                Description  = string.IsNullOrWhiteSpace(r.Notes) ? (r.RevenueTypeName ?? "Prihod") : r.Notes,
                Category     = r.RevenueTypeName ?? "Ostalo",
                Amount       = r.Amount,
                Date         = r.TransactionDate,
                Type         = TransactionType.Income,
                Scope        = r.GroupId == personalGroupId ? TransactionScope.Personal : TransactionScope.Family,
                IsApiRevenue = true,
            });

        foreach (var b in budgets.Where(b => b.UserGroupId == api.PersonalUserGroupId))
        {
            var spent = costs
                .Where(c => c.GroupId == personalGroupId &&
                            c.TransactionDate.Month == b.Month && c.TransactionDate.Year == b.Year)
                .Sum(c => c.Amount);
            Budgets.Add(new Budget
            {
                Id             = b.Id,
                Name           = FormatBudgetName(b.Year, b.Month),
                Category       = string.Empty,
                Limit          = b.Amount,
                Spent          = spent,
                ApiMonth       = b.Month,
                ApiYear        = b.Year,
                ApiUserGroupId = b.UserGroupId,
            });
        }

        FamilyMembers.Clear();
        var avatarColors = new[] { "#3B82F6", "#22C55E", "#F59E0B", "#8B5CF6", "#EF4444", "#06B6D4" };

        // Dohvati stvarne članove obiteljske (dijeljene) grupe — istu grupu u koju se
        // šalju pozivnice. GetAllUserGroups vraća samo članstva trenutnog korisnika,
        // pa za popis svih članova koristimo GetMemebersByGroup.
        var familyGroupId = api.FamilyGroupId != 0 ? api.FamilyGroupId : api.PersonalGroupId;
        var members = familyGroupId > 0
            ? await api.GetAsync<ApiUserGroup[]>($"/api/UserGroup/GetMemebersByGroup/{familyGroupId}") ?? []
            : Array.Empty<ApiUserGroup>();

        bool currentUserIsOwner = members.Any(m => m.UserId == api.UserId && m.Role == FamilyMember.RoleVlasnik);

        int colorIdx = 0;
        foreach (var ug in members.OrderByDescending(m => m.Role).ThenBy(m => m.Username))
        {
            bool isCurrent = ug.UserId == api.UserId;
            int roleId = ug.Role == 0 ? FamilyMember.RoleClan : ug.Role;
            FamilyMembers.Add(new FamilyMember
            {
                Name          = ug.Username ?? "Korisnik",
                Email         = string.Empty,
                UserId        = ug.UserId,
                UserGroupId   = ug.Id,
                RoleId        = roleId,
                IsCurrentUser = isCurrent,
                CanManage     = currentUserIsOwner && !isCurrent && roleId != FamilyMember.RoleVlasnik,
                AvatarColor   = avatarColors[colorIdx++ % avatarColors.Length],
                TotalIncome   = revenues.Where(r => r.UserId == ug.UserId).Sum(r => r.Amount),
                TotalSpent    = costs.Where(c => c.UserId == ug.UserId).Sum(c => c.Amount),
            });
        }
    }
}
