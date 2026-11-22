using MediatR;
using Microsoft.EntityFrameworkCore;
using ZeeAcom.Common.Models;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Listings.Queries;

/// <summary>Suburbs/areas that currently have available listings, for the search filter.</summary>
public class GetListingSuburbsQuery : IRequest<Result<IEnumerable<string>>>
{
}

public class GetListingSuburbsQueryHandler(DatabaseContext databaseContext)
    : IRequestHandler<GetListingSuburbsQuery, Result<IEnumerable<string>>>
{
    public async Task<Result<IEnumerable<string>>> Handle(GetListingSuburbsQuery request, CancellationToken cancellationToken)
    {
        var suburbs = await databaseContext.Properties
            .AsNoTracking()
            .PubliclyVisible()
            .Select(p => p.Suburb)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<string>>.Success(suburbs);
    }
}
