using ZeeAcom.Common.Entities;

namespace ZeeAcom.Core.Rooms.Queries;

public static class RoomFilter
{
    public static IQueryable<Room> FilterByProperty(this IQueryable<Room> query, Guid? propertyId)
        => propertyId is null || propertyId == Guid.Empty ? query : query.Where(x => x.PropertyId == propertyId);

    public static IQueryable<Room> FilterByAvailable(this IQueryable<Room> query, bool? available)
        => available is null ? query : query.Where(x => x.Available == available);

    /// <summary>Only rooms in properties owned by <paramref name="ownerId"/> (landlord requests).</summary>
    public static IQueryable<Room> FilterByOwner(this IQueryable<Room> query, Guid? ownerId)
        => ownerId is null ? query : query.Where(x => x.Property.OwnerId == ownerId);
}
