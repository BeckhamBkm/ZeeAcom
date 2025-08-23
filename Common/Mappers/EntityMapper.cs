using Riok.Mapperly.Abstractions;
using ZeeAcom.Common.Models.EntityModels;

namespace ZeeAcom.Common.Mappers
{
    [Mapper]
    public static partial class EntityMapper
    {
        public static partial Entity Map(this EntityModel model);
        public static partial Entity Map(this CreateEntityModel model);

        public static partial EntityModel Map(this Entity entity);

        public static IEnumerable<EntityModel> Map(this List<Entity> entities)
            => entities.Select(x => x.Map());

        public static IEnumerable<Entity> Map(this List<EntityModel> entities)
            => entities.Select(x => x.Map());

    }
}
