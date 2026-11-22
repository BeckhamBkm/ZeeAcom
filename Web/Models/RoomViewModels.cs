using ZeeAcom.Common.Models.PropertyModels;
using ZeeAcom.Common.Models.RoomModels;

namespace ZeeAcom.Web.Models;

public sealed class RoomsPageViewModel
{
    public required PropertyModel Property { get; set; }
    public IEnumerable<RoomModel> Rooms { get; set; } = [];

    /// <summary>Admin sees the owner and goes back to the admin Properties list; a landlord goes back to My listings.</summary>
    public bool IsAdmin { get; set; }
}

public sealed class RoomFormViewModel
{
    public Guid PropertyId { get; set; }
    public string PropertyName { get; set; } = string.Empty;

    /// <summary>Null when adding a new room.</summary>
    public Guid? Id { get; set; }

    public SaveRoomModel Room { get; set; } = new();
}
