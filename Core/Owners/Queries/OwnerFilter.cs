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

    }
}
