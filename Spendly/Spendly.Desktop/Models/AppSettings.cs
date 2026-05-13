namespace Spendly.Desktop.Models;

public class AppSettings
{
    public string ApiUrl { get; set; } = "https://localhost:5001/api";
    public string Currency { get; set; } = "EUR";
    public bool BudgetWarningAlerts { get; set; } = true;
    public bool BudgetCriticalAlerts { get; set; } = true;
    public bool StartMinimized { get; set; } = false;
}
