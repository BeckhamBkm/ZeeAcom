using ZeeAcom.Common.Entities;
using ZeeAcom.Common.Models.ListingModels;

namespace ZeeAcom.Core.Listings.Queries;

public static class ListingFilter
{
    /// <summary>
    /// What the public may see. Every public query goes through this, so moderation rules
    /// (e.g. hidden listings, unapproved or blocked landlords) only need adding here.
    /// </summary>
    public static IQueryable<Property> PubliclyVisible(this IQueryable<Property> query)
        => query.Where(p => p.Active);

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
