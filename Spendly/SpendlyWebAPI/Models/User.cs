using System;
using System.Collections.Generic;

namespace SpendlyWebAPI.Models;

public partial class User
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string PasswordSalt { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual ICollection<Cost> Costs { get; set; } = new List<Cost>();

    public virtual ICollection<Revenue> Revenues { get; set; } = new List<Revenue>();

    public virtual ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
}
