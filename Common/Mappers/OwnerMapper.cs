using Riok.Mapperly.Abstractions;
using ZeeAcom.Common.Entities;
using ZeeAcom.Common.Models.OwnerModels;
using ZeeAcom.Common.Models.PropertyModels;
using ZeeAcom.Common.Models.RoomModels;

namespace ZeeAcom.Common.Mappers;

[Mapper]
public static partial class OwnerMapper
{
    // The login link (UserId/User) is never set or exposed through these models.
    [MapperIgnoreTarget(nameof(Owner.UserId))]
    [MapperIgnoreTarget(nameof(Owner.User))]
    public static partial Owner Map(this OwnerModel model);

    [MapperIgnoreTarget(nameof(Owner.Id))]
    [MapperIgnoreTarget(nameof(Owner.UserId))]
    [MapperIgnoreTarget(nameof(Owner.User))]
    public static partial Owner Map(this CreateOwnerModel model);

    [MapperIgnoreSource(nameof(Owner.UserId))]
    [MapperIgnoreSource(nameof(Owner.User))]
    public static partial OwnerModel Map(this Owner owner);

    public static IEnumerable<OwnerModel> Map(this List<Owner> owners)
        => owners.Select(x => x.Map());

    public static IEnumerable<Owner> Map(this List<OwnerModel> owners)
        => owners.Select(x => x.Map());

    // Nested mappings for OwnerModel.Properties. Each property's Owner is skipped
    // so Owner -> Properties -> Owner doesn't loop forever.
    [MapperIgnoreTarget(nameof(PropertyModel.Owner))]
    [MapperIgnoreSource(nameof(Property.Owner))]
    private static partial PropertyModel MapPropertyToModel(Property property);

    [MapperIgnoreTarget(nameof(Property.Owner))]
    [MapperIgnoreSource(nameof(PropertyModel.Owner))]
    private static partial Property MapModelToProperty(PropertyModel property);

    [MapperIgnoreTarget(nameof(Property.Id))]
    [MapperIgnoreTarget(nameof(Property.Owner))]
    [MapperIgnoreTarget(nameof(Property.OwnerId))]
    [MapperIgnoreTarget(nameof(Property.Rooms))]
    [MapperIgnoreSource(nameof(CreatePropertyModel.OwnerId))]
    private static partial Property MapNewProperty(CreatePropertyModel property);

    [MapperIgnoreSource(nameof(Room.Property))]
    private static partial RoomModel MapRoomModel(Room room);

    [MapperIgnoreTarget(nameof(Room.Property))]
    private static partial Room MapRoom(RoomModel room);

    [MapperIgnoreTarget(nameof(Picture.Property))]
    private static partial Picture MapPicture(PictureModel picture);

    [MapperIgnoreSource(nameof(Picture.Property))]
    private static partial PictureModel MapPictureModel(Picture picture);
}
