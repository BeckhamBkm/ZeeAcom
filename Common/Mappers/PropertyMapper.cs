using Riok.Mapperly.Abstractions;
using ZeeAcom.Common.Entities;
using ZeeAcom.Common.Models.OwnerModels;
using ZeeAcom.Common.Models.PropertyModels;
using ZeeAcom.Common.Models.RoomModels;

namespace ZeeAcom.Common.Mappers
{
    [Mapper]
    public static partial class PropertyMapper
    {
        [MapperIgnoreTarget(nameof(Property.Owner))]
        [MapperIgnoreSource(nameof(PropertyModel.Owner))]
        public static partial Property Map(this PropertyModel model);

        [MapperIgnoreTarget(nameof(Property.Id))]
        [MapperIgnoreTarget(nameof(Property.Owner))]
        [MapperIgnoreTarget(nameof(Property.Rooms))]
        public static partial Property Map(this CreatePropertyModel model);

        public static partial PropertyModel Map(this Property property);

        public static IEnumerable<PropertyModel> Map(this List<Property> properties)
            => properties.Select(x => x.Map());

        public static IEnumerable<Property> Map(this List<PropertyModel> properties)
            => properties.Select(x => x.Map());

        // Nested mapping for PropertyModel.Owner. The owner's Properties are skipped
        // so Property -> Owner -> Properties doesn't loop forever.
        [MapperIgnoreTarget(nameof(OwnerModel.Properties))]
        [MapperIgnoreSource(nameof(Owner.Properties))]
        [MapperIgnoreSource(nameof(Owner.UserId))]
        [MapperIgnoreSource(nameof(Owner.User))]
        private static partial OwnerModel MapOwner(Owner owner);

        [MapperIgnoreTarget(nameof(Picture.Property))]
        private static partial Picture MapPicture(PictureModel picture);

        [MapperIgnoreSource(nameof(Picture.Property))]
        private static partial PictureModel MapPictureModel(Picture picture);

        // Nested mappings for PropertyModel.Rooms (each room's Property back-reference is skipped).
        [MapperIgnoreSource(nameof(Room.Property))]
        private static partial RoomModel MapRoomModel(Room room);

        [MapperIgnoreTarget(nameof(Room.Property))]
        private static partial Room MapRoom(RoomModel room);
    }
}
