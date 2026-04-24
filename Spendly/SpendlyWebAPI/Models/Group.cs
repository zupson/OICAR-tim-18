using System;
using System.Collections.Generic;

namespace SpendlyWebAPI.Models;

public partial class Group
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsPersonal { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Cost> Costs { get; set; } = new List<Cost>();

    public virtual ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();

    public virtual ICollection<Revenue> Revenues { get; set; } = new List<Revenue>();

    public virtual ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
}
