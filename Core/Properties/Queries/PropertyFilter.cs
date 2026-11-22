using ZeeAcom.Common.Entities;

namespace ZeeAcom.Core.Properties.Queries;

public static class PropertyFilter
{
    public static IQueryable<Property> FilterByName(this IQueryable<Property> query, string? name)
        => string.IsNullOrWhiteSpace(name) ? query : query.Where(x => x.Name.Contains(name));

    public static IQueryable<Property> FilterBySuburb(this IQueryable<Property> query, string? suburb)
        => string.IsNullOrWhiteSpace(suburb) ? query : query.Where(x => x.Suburb.Contains(suburb));

    public static IQueryable<Property> FilterByOwner(this IQueryable<Property> query, Guid? ownerId)
        => ownerId is null || ownerId == Guid.Empty ? query : query.Where(x => x.OwnerId == ownerId);

    public static IQueryable<Property> FilterByActive(this IQueryable<Property> query, bool? active)
        => active is null ? query : query.Where(x => x.Active == active);
}
