namespace SpendlyWebAPI.Models;

public partial class RevenueType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int GroupId { get; set; }

    public virtual Group Group { get; set; } = null!;

    public virtual ICollection<Revenue> Revenues { get; set; } = new List<Revenue>();
}
