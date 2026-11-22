namespace ZeeAcom.Common.Models.PictureModels;

public sealed class PictureModel
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;

    /// <summary>Small version for lists. Falls back to <see cref="Url"/> for older pictures without one.</summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>Optional label, e.g. "Main bedroom", "Kitchen".</summary>
    public string? Caption { get; set; }

    /// <summary>Lowest first; the first picture is the cover photo.</summary>
    public int SortOrder { get; set; }

    public DateTime DateUploaded { get; set; }
    public Guid PropertyId { get; set; }
}
