namespace ZeeAcom.Common.Entities;

/// <summary>A message from someone interested in a property, sent from its public listing page.</summary>
public class Enquiry
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string CellPhone { get; set; } = null!;

    public string? Email { get; set; }

    public string Message { get; set; } = null!;

    public DateTime DateCreated { get; set; }

    public Guid PropertyId { get; set; }

    public virtual Property Property { get; set; } = null!;
}
