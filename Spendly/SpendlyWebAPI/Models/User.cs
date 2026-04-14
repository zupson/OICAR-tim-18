namespace SpendlyWebAPI.Models;

public partial class User
{
    public int Id { get; set; }

    public string FistName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string PasswordSalt { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual ICollection<Cost> Costs { get; set; } = new List<Cost>();

    public virtual ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();

    public virtual ICollection<Revenue> Revenues { get; set; } = new List<Revenue>();

    public virtual ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
