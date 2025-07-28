using Riok.Mapperly.Abstractions;
using ZeeAcom.Common.Models.EntityModels;

namespace ZeeAcom.Common.Mappers
{
    [Mapper]
    public static partial class EntityMapper
    {
        public static partial Entity Map(this EntityModel model);
        public static partial EntityModel Map(this Entity entity);
    }
}
