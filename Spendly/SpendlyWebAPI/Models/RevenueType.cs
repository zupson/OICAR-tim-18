using System;
using System.Collections.Generic;

namespace SpendlyWebAPI.Models;

public partial class RevenueType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Revenue> Revenues { get; set; } = new List<Revenue>();
}
