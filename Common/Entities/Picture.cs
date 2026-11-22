namespace ZeeAcom.Common.Entities;

public partial class Picture
{
    public Guid Id { get; set; }

    public string Url { get; set; } = null!;

    /// <summary>Small version for lists. Null for older pictures (use <see cref="Url"/>).</summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>Optional label, e.g. "Main bedroom", "Kitchen".</summary>
    public string? Caption { get; set; }

    /// <summary>Lowest first; the first picture is the property's cover photo.</summary>
    public int SortOrder { get; set; }

    public DateTime DateUploaded { get; set; }

    public Guid PropertyId { get; set; }

    public virtual Property Property { get; set; } = null!;
}
