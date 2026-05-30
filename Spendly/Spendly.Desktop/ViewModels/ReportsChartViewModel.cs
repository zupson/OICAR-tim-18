using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Spendly.Desktop.Models;
using Spendly.Desktop.Services;

namespace Spendly.Desktop.ViewModels;

public partial class MonthPill : ObservableObject
{
    [ObservableProperty] private bool _isSelected;
    public int    Month { get; init; }
    public int    Year  { get; init; }
    public string Label { get; init; } = string.Empty;
}

public record CatBar(string Name, decimal Amount, int Pct, string Color, string Symbol, decimal Rate)
{
    public int Remaining => Math.Max(0, 100 - Pct);
}

public record TrendPoint(string Label, decimal Inc, decimal Exp,
    int IncH, int IncRemaining, int ExpH, int ExpRemaining, string Symbol, decimal Rate);

public record BreakdownRow(string Desc, string Cat, string DateStr, bool IsIncome, decimal Amount, string Symbol, decimal Rate);

public partial class ReportsChartViewModel : ObservableObject
{
    private readonly DataCache _data;

    private static readonly string[] Colors      = ["#3B82F6", "#22C55E", "#F59E0B", "#8B5CF6", "#EF4444", "#06B6D4"];
    private static readonly string[] MonthsShort = ["Sij", "Velj", "Ožu", "Tra", "Svi", "Lip", "Srp", "Kol", "Ruj", "Lis", "Stu", "Pro"];

    public ObservableCollection<MonthPill> MonthPills { get; } = [];

    [ObservableProperty] private string  _headerLabel  = string.Empty;
    [ObservableProperty] private decimal _totalIncome;
    [ObservableProperty] private decimal _totalExpense;
    [ObservableProperty] private decimal _netBalance;
    [ObservableProperty] private List<CatBar>       _expByCategory = [];
    [ObservableProperty] private List<CatBar>       _incByCategory = [];
    [ObservableProperty] private List<TrendPoint>   _trend         = [];
    [ObservableProperty] private List<BreakdownRow> _breakdown     = [];

    public bool IsNetPositive  => NetBalance >= 0;
    public bool HasNoExpByCat  => ExpByCategory.Count == 0;
    public bool HasNoIncByCat  => IncByCategory.Count == 0;
    public bool HasNoBreakdown => Breakdown.Count == 0;

    public string  CurrencySymbol => _data.CurrencySymbol;
    public decimal ExchangeRate   => _data.ExchangeRate;

    private int _selMonth = DateTime.Now.Month;
    private int _selYear  = DateTime.Now.Year;

    public ReportsChartViewModel(DataCache data)
    {
        _data = data;
        data.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CurrencySymbol));
            OnPropertyChanged(nameof(ExchangeRate));
            Recalculate();
        };
        data.Transactions.CollectionChanged += (_, _) => Recalculate();
        BuildPills();
        Recalculate();
    }

    private void BuildPills()
    {
        MonthPills.Clear();
        var now = DateTime.Now;
        for (int i = 5; i >= 0; i--)
        {
            var d = now.AddMonths(-i);
            MonthPills.Add(new MonthPill
            {
                Month      = d.Month,
                Year       = d.Year,
                Label      = d.Year == now.Year ? MonthsShort[d.Month - 1] : $"{MonthsShort[d.Month - 1]} {d.Year}",
                IsSelected = i == 0,
            });
        }
    }

    [RelayCommand]
    private void SelectPill(MonthPill pill)
    {
        foreach (var p in MonthPills) p.IsSelected = false;
        pill.IsSelected = true;
        _selMonth = pill.Month;
        _selYear  = pill.Year;
        Recalculate();
    }

    private void Recalculate()
    {
        var sym  = CurrencySymbol;
        var rate = ExchangeRate;

        var mExp = _data.Transactions
            .Where(t => t.Type == TransactionType.Expense && t.Date.Month == _selMonth && t.Date.Year == _selYear)
            .ToList();
        var mInc = _data.Transactions
            .Where(t => t.Type == TransactionType.Income && t.Date.Month == _selMonth && t.Date.Year == _selYear)
            .ToList();

        TotalExpense = mExp.Sum(t => t.Amount);
        TotalIncome  = mInc.Sum(t => t.Amount);
        NetBalance   = TotalIncome - TotalExpense;

        // Category bars — expenses
        var expG    = mExp.GroupBy(t => t.Category).Select(g => (Name: g.Key, Amt: g.Sum(t => t.Amount))).OrderByDescending(x => x.Amt).Take(6).ToList();
        var expTot  = expG.Sum(x => x.Amt) is var et && et == 0 ? 1m : et;
        ExpByCategory = expG.Select((x, i) => new CatBar(x.Name, x.Amt,
            Math.Max(4, (int)Math.Round(x.Amt / expTot * 100)), Colors[i % Colors.Length], sym, rate)).ToList();

        // Category bars — revenues
        var incG    = mInc.GroupBy(t => t.Category).Select(g => (Name: g.Key, Amt: g.Sum(t => t.Amount))).OrderByDescending(x => x.Amt).Take(6).ToList();
        var incTot  = incG.Sum(x => x.Amt) is var it && it == 0 ? 1m : it;
        IncByCategory = incG.Select((x, i) => new CatBar(x.Name, x.Amt,
            Math.Max(4, (int)Math.Round(x.Amt / incTot * 100)), Colors[i % Colors.Length], sym, rate)).ToList();

        // 6-month trend
        var months  = Enumerable.Range(0, 6).Select(i => DateTime.Now.AddMonths(i - 5)).ToList();
        var vals    = months.Select(d => new
        {
            Label = MonthsShort[d.Month - 1],
            Inc   = _data.Transactions.Where(t => t.Type == TransactionType.Income  && t.Date.Month == d.Month && t.Date.Year == d.Year).Sum(t => t.Amount),
            Exp   = _data.Transactions.Where(t => t.Type == TransactionType.Expense && t.Date.Month == d.Month && t.Date.Year == d.Year).Sum(t => t.Amount),
        }).ToList();
        var maxVal  = vals.Max(v => Math.Max(v.Inc, v.Exp)) is var mv && mv == 0 ? 1m : mv;
        Trend = vals.Select(v =>
        {
            var ih = (int)Math.Round(v.Inc / maxVal * 100);
            var eh = (int)Math.Round(v.Exp / maxVal * 100);
            return new TrendPoint(v.Label, v.Inc, v.Exp, ih, 100 - ih, eh, 100 - eh, sym, rate);
        }).ToList();

        // Breakdown table
        var bExp = mExp.Select(t => new BreakdownRow(
            string.IsNullOrWhiteSpace(t.Description) ? t.Category : t.Description,
            t.Category, t.Date.ToString("d. M. yyyy."), false, t.Amount, sym, rate));
        var bInc = mInc.Select(t => new BreakdownRow(
            string.IsNullOrWhiteSpace(t.Description) ? t.Category : t.Description,
            t.Category, t.Date.ToString("d. M. yyyy."), true, t.Amount, sym, rate));
        Breakdown = bExp.Concat(bInc).OrderByDescending(x => x.DateStr).Take(20).ToList();

        HeaderLabel = $"{MonthsShort[_selMonth - 1]} {_selYear}. — Financijski pregled";
        OnPropertyChanged(nameof(IsNetPositive));
        OnPropertyChanged(nameof(HasNoExpByCat));
        OnPropertyChanged(nameof(HasNoIncByCat));
        OnPropertyChanged(nameof(HasNoBreakdown));
    }
}
