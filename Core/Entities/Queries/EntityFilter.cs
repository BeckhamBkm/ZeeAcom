namespace ZeeAcom.Core.Entities.Queries;

// `Entity` here is the EF type ZeeAcom.Entity (the one DatabaseContext maps).
public static class EntityFilter
{
    public static IQueryable<Entity> FilterByName(this IQueryable<Entity> query, string? name)
        => string.IsNullOrWhiteSpace(name) ? query : query.Where(x => x.Name.Contains(name));

    public static IQueryable<Entity> FilterBySuburb(this IQueryable<Entity> query, string? suburb)
        => string.IsNullOrWhiteSpace(suburb) ? query : query.Where(x => x.Suburb.Contains(suburb));

    public static IQueryable<Entity> FilterByOwner(this IQueryable<Entity> query, Guid? ownerId)
        => ownerId is null || ownerId == Guid.Empty ? query : query.Where(x => x.OwnerId == ownerId);

    public static IQueryable<Entity> FilterByActive(this IQueryable<Entity> query, bool? active)
        => active is null ? query : query.Where(x => x.Active == active);
}
