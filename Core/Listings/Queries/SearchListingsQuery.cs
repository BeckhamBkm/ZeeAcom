using MediatR;
using Microsoft.EntityFrameworkCore;
using ZeeAcom.Common.Extensions;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.ListingModels;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Listings.Queries;

/// <summary>Public search over available listings. <see cref="Result{T}.Count"/> is the total before paging.</summary>
public class SearchListingsQuery : IRequest<Result<IEnumerable<ListingModel>>>
{
    public required SearchListingsModel Data { get; set; }
}

public class SearchListingsQueryHandler(DatabaseContext databaseContext)
    : IRequestHandler<SearchListingsQuery, Result<IEnumerable<ListingModel>>>
{
    public async Task<Result<IEnumerable<ListingModel>>> Handle(SearchListingsQuery request, CancellationToken cancellationToken)
    {
        var search = request.Data;
        var query = databaseContext.Properties
            .AsNoTracking()
            .PubliclyVisible()
            .FilterByText(search.Text)
            .FilterBySuburb(search.Suburb)
            .FilterByMaxRent(search.MaxRent);

        var count = await query.CountAsync(cancellationToken);
        var items = await query
            .SortBy(search.Sort)
            .ApplyPaging(search.PagingArgs)
            .Select(p => new ListingModel
            {
                Id = p.Id,
                Name = p.Name,
                Address = p.Address,
                Suburb = p.Suburb,
                MonthlyRent = p.MonthlyRent,
                DateCreated = p.DateCreated,
                PhotoCount = p.Pictures.Count,
                CoverThumbnailUrl = p.Pictures
                    .OrderBy(x => x.SortOrder).ThenBy(x => x.DateUploaded)
                    .Select(x => x.ThumbnailUrl ?? x.Url)
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<ListingModel>>.Success(items, count);
    }
}
