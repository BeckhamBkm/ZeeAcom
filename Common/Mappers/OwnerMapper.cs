using Riok.Mapperly.Abstractions;
using ZeeAcom.Common.Models.OwnerModels;
namespace ZeeAcom.Common.Mappers;

[Mapper]
public static partial class OwnerMapper
{
    public static partial Owner Map(this OwnerModel model);
    public static partial Owner Map(this CreateOwnerModel model);
    public static partial OwnerModel Map(this Owner owner);
    public static IEnumerable<OwnerModel> Map(this List<Owner> owners)
        => owners.Select(x => x.Map());

    public static IEnumerable<Owner> Map(this List<OwnerModel> owners)
        => owners.Select(x => x.Map());
}

