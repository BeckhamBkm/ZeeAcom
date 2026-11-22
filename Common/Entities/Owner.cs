namespace ZeeAcom.Common.Entities;

/// <summary>
/// A landlord who owns one or more properties.
/// </summary>
public partial class Owner
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string CellPhone { get; set; } = null!;

    public string Email { get; set; } = null!;
    //public DateTime? DateCreated { get; set; }

    /// <summary>
    /// The landlord's login account. Null for owners added by the admin who don't log in.
    /// </summary>
    public string? UserId { get; set; }

    public virtual ApplicationUser? User { get; set; }

    /// <summary>Set by the admin. New landlords start as Pending; only Approved landlords' listings are public.</summary>
    public OwnerStatus Status { get; set; } = OwnerStatus.Pending;

    public virtual ICollection<Property> Properties { get; set; } = new List<Property>();
}
