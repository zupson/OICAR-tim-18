namespace SpendlyWebAPI.Models;

public partial class Cost
{
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public DateTime TransactionDate { get; set; }

    public string? Notes { get; set; }

    public bool IsDeleted { get; set; }

    public int UserId { get; set; }

    public int CurrencyId { get; set; }

    public int CostTypeId { get; set; }

    public virtual CostType CostType { get; set; } = null!;

    public virtual Currency Currency { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();
}
