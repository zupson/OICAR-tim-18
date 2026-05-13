namespace Spendly.Desktop.Models;

public class Budget
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Limit { get; set; }
    public decimal Spent { get; set; }

    public decimal Remaining => Limit - Spent;
    public decimal RemainingAbsolute => Math.Abs(Remaining);
    public bool IsOverBudget => Spent > Limit;
    public string RemainingLabel => IsOverBudget ? "Prekoračeno" : "Preostalo";
    public double ProgressPercent => Limit > 0 ? Math.Min((double)(Spent / Limit) * 100, 100) : 0;
    public AlertLevel AlertLevel => ProgressPercent >= 100 ? AlertLevel.Critical
        : ProgressPercent >= 80 ? AlertLevel.Warning
        : AlertLevel.Normal;
}

public enum AlertLevel { Normal, Warning, Critical }
