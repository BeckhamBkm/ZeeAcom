using ZeeAcom.Common.Entities;
using ZeeAcom.Common.Models.ListingModels;

namespace ZeeAcom.Core.Listings.Queries;

public static class ListingFilter
{
    /// <summary>
    /// What the public may see. Every public query goes through this: the landlord has it marked
    /// available, the admin hasn't hidden it, and the landlord is approved (not pending, hidden or blocked).
    /// PropertyModel.IsPublic shows the same rule on the admin and landlord pages.
    /// </summary>
    public static IQueryable<Property> PubliclyVisible(this IQueryable<Property> query)
        => query.Where(p => p.Active && !p.HiddenByAdmin && p.Owner.Status == OwnerStatus.Approved);

    public static IQueryable<Property> FilterByText(this IQueryable<Property> query, string? text)
        => string.IsNullOrWhiteSpace(text)
            ? query
            : query.Where(p => p.Name.Contains(text.Trim()) || p.Address.Contains(text.Trim()));

    public static IQueryable<Property> FilterBySuburb(this IQueryable<Property> query, string? suburb)
        => string.IsNullOrWhiteSpace(suburb) ? query : query.Where(p => p.Suburb == suburb.Trim());

    public static IQueryable<Property> FilterByMaxRent(this IQueryable<Property> query, decimal? maxRent)
        => maxRent is null ? query : query.Where(p => p.MonthlyRent != null && p.MonthlyRent <= maxRent);

    public static IQueryable<Property> SortBy(this IQueryable<Property> query, ListingSort sort) => sort switch
    {
        // Listings without a rent go last either way.
        ListingSort.RentLowToHigh => query.OrderBy(p => p.MonthlyRent == null).ThenBy(p => p.MonthlyRent).ThenByDescending(p => p.DateCreated),
        ListingSort.RentHighToLow => query.OrderBy(p => p.MonthlyRent == null).ThenByDescending(p => p.MonthlyRent).ThenByDescending(p => p.DateCreated),
        _ => query.OrderByDescending(p => p.DateCreated),
    };
}
