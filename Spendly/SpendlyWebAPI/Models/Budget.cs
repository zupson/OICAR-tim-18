using System;
using System.Collections.Generic;

namespace SpendlyWebAPI.Models;

public partial class Budget
{
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public bool IsDeleted { get; set; }

    public int UserGroupId { get; set; }

    public int Currency { get; set; }

    public virtual UserGroup UserGroup { get; set; } = null!;
}
