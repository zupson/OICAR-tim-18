using System;
using System.Collections.Generic;

namespace SpendlyWebAPI.Models;

public partial class UserGroup
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int GroupId { get; set; }

    public int Role { get; set; }

    public DateTime JoinedAt { get; set; }

    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

    public virtual Group Group { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
