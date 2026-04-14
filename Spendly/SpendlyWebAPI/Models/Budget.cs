namespace SpendlyWebAPI.Models;

public partial class Budget
{
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public bool IsDeleted { get; set; }

    public int UserGroupId { get; set; }

    public int CurrencyId { get; set; }

    public virtual Currency Currency { get; set; } = null!;

    public virtual UserGroup UserGroup { get; set; } = null!;
}
