using System;
using System.Collections.Generic;

namespace SpendlyWebAPI.Models;

public partial class Invitation
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Token { get; set; } = null!;

    public DateTime? ClaimedAt { get; set; }

    public DateTime ExpiredAt { get; set; }

    public bool IsDeleted { get; set; }

    public int GroupId { get; set; }

    public virtual Group Group { get; set; } = null!;
}
