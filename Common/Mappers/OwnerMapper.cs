using Riok.Mapperly.Abstractions;
using ZeeAcom.Common.Models.OwnerModels;
namespace ZeeAcom.Common.Mappers;

[Mapper]
public static partial class OwnerMapper
{
    public static partial Owner Map(this OwnerModel model);
    public static partial OwnerModel Map(this Owner entity);
}
