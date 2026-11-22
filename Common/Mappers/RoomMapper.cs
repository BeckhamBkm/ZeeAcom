using Riok.Mapperly.Abstractions;
using ZeeAcom.Common.Entities;
using ZeeAcom.Common.Models.RoomModels;

namespace ZeeAcom.Common.Mappers;

[Mapper]
public static partial class RoomMapper
{
    [MapperIgnoreSource(nameof(Room.Property))]
    public static partial RoomModel Map(this Room room);

    public static IEnumerable<RoomModel> Map(this List<Room> rooms)
        => rooms.Select(x => x.Map());

    /// <summary>Copies the editable fields onto an existing room.</summary>
    public static void Apply(this SaveRoomModel model, Room room)
    {
        room.Name = model.Name.Trim();
        room.MonthlyRent = model.MonthlyRent;
        room.Available = model.Available;
        room.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
    }
}
