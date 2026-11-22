using ZeeAcom.Common.Models.PropertyModels;

namespace ZeeAcom.Web.Models;

public sealed class PhotosPageViewModel
{
    /// <summary>The property, with its pictures in display order (cover first).</summary>
    public required PropertyModel Property { get; set; }

    /// <summary>Admin goes back to the admin Properties list; a landlord goes back to My listings.</summary>
    public bool IsAdmin { get; set; }

    /// <summary>Files skipped by the last upload, with the reason.</summary>
    public IReadOnlyList<string> UploadErrors { get; set; } = [];
}
