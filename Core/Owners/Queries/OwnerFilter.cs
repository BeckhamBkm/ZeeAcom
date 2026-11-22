using Microsoft.EntityFrameworkCore;
using ZeeAcom.Common.Entities;

namespace ZeeAcom.Core.Owners.Queries
{
    public static class OwnerFilter
    {
        public static IQueryable<Owner> FilterByFirstName(this IQueryable<Owner> query,string? firstName)
            => string.IsNullOrWhiteSpace(firstName)? query : query.Where(x=>x.FirstName.Contains(firstName));

        public static IQueryable<Owner> FilterByLastName(this IQueryable<Owner> query, string? lastName)
           => string.IsNullOrWhiteSpace(lastName) ? query : query.Where(x => x.LastName.Contains(lastName));

        public static IQueryable<Owner> FilterByStatus(this IQueryable<Owner> query, OwnerStatus? status)
           => status is null ? query : query.Where(x => x.Status == status);

        /// <summary>Landlords waiting for approval first, then by name.</summary>
        public static IQueryable<Owner> PendingFirst(this IQueryable<Owner> query)
           => query.OrderBy(x => x.Status == OwnerStatus.Pending ? 0 : 1).ThenBy(x => x.LastName).ThenBy(x => x.FirstName);

    }
}
