using ZeeAcom.Common.Models.OwnerModels;
using ZeeAcom.Common.Models.PictureModels;

namespace ZeeAcom.Common.Models.PropertyModels;

public sealed class PropertyModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Suburb { get; set; } = string.Empty;
    public bool Active { get; set; }

    /// <summary>Monthly rent in rand (null for older listings without one).</summary>
    public decimal? MonthlyRent { get; set; }

    public DateTime DateCreated { get; set; }
    public Guid SessionId { get; set; }

    /// <summary>
    /// Collection of pictures of the property (cover photo first)
    /// </summary>
    public ICollection<PictureModel> Pictures { get; set; } = new List<PictureModel>();

    /// <summary>Number of photos (filled by list queries, which don't load every picture).</summary>
    public int PhotoCount { get; set; }

    /// <summary>Thumbnail of the cover photo (filled by list queries).</summary>
    public string? CoverThumbnailUrl { get; set; }

    public Guid OwnerId { get; set; }
    public OwnerModel? Owner { get; set; }
}
