using ZeeAcom.Common.Models.OwnerModels;
using ZeeAcom.Common.Models.RoomModels;

namespace ZeeAcom.Common.Models.PropertyModels;

public sealed class PropertyModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Suburb { get; set; } = string.Empty;
    public bool Active { get; set; }
    public DateTime DateCreated { get; set; }
    public Guid SessionId { get; set; }

    /// <summary>
    /// Collection of pictures of the property
    /// </summary>
    public ICollection<PictureModel> Pictures { get; set; } = new List<PictureModel>();

    /// <summary>
    /// Rooms in the property
    /// </summary>
    public ICollection<RoomModel> Rooms { get; set; } = new List<RoomModel>();

    public Guid OwnerId { get; set; }
    public OwnerModel? Owner { get; set; }
}
