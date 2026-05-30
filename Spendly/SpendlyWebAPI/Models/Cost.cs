using System;
using System.Collections.Generic;

namespace SpendlyWebAPI.Models;

public partial class Cost
{
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public DateTime TransactionDate { get; set; }

    public string? Notes { get; set; }

    public bool IsDeleted { get; set; }

    public int UserId { get; set; }

    public int Currency { get; set; }

    public int CostTypeId { get; set; }

    public int GroupId { get; set; }

    public virtual CostType CostType { get; set; } = null!;

    public virtual Group Group { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
