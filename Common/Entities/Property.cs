namespace ZeeAcom.Common.Entities;

/// <summary>
/// A listed accommodation property (house, flat, cottage...). Stored in the "Property" table.
/// </summary>
public partial class Property
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Suburb { get; set; } = null!;

    public bool Active { get; set; }

    /// <summary>Monthly rent in rand. Null for older listings that don't have one yet.</summary>
    public decimal? MonthlyRent { get; set; }

    public DateTime DateCreated { get; set; }

    public Guid SessionId { get; set; }

    public Guid OwnerId { get; set; }

    public virtual Owner Owner { get; set; } = null!;

    public virtual ICollection<Picture> Pictures { get; set; } = new List<Picture>();
}
