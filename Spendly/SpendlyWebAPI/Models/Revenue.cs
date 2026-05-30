using System;
using System.Collections.Generic;

namespace SpendlyWebAPI.Models;

public partial class Revenue
{
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public DateTime TransactionDate { get; set; }

    public string? Notes { get; set; }

    public bool IsDeleted { get; set; }

    public int UserId { get; set; }

    public int Currency { get; set; }

    public int RevenueTypeId { get; set; }

    public int GroupId { get; set; }

    public virtual Group Group { get; set; } = null!;

    public virtual RevenueType RevenueType { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
