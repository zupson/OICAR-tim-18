using System;
using System.Collections.Generic;

namespace SpendlyWebAPI.Models;

public partial class CostType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int GroupId { get; set; }

    public virtual ICollection<Cost> Costs { get; set; } = new List<Cost>();

    public virtual Group Group { get; set; } = null!;
}
