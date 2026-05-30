namespace Spendly.Desktop.Models;

public class AppSettings
{
    public string ApiUrl { get; set; } = "http://localhost:5153";
    public string Currency { get; set; } = "EUR";
    public bool BudgetWarningAlerts { get; set; } = true;
    public bool BudgetCriticalAlerts { get; set; } = true;
    public bool StartMinimized { get; set; } = false;
}
