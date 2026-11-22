namespace ZeeAcom.Common.Entities;

/// <summary>
/// A rentable room (or unit) within a property, e.g. "Room 1", "Bachelor flat". Stored in the "Room" table.
/// </summary>
public class Room
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    /// <summary>Monthly rent in rand.</summary>
    public decimal MonthlyRent { get; set; }

    public bool Available { get; set; }

    public string? Description { get; set; }

    public DateTime DateCreated { get; set; }

    public Guid PropertyId { get; set; }

    public virtual Property Property { get; set; } = null!;
}
