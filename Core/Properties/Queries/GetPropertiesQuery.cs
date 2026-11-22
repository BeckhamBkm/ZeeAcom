using MediatR;
using Microsoft.EntityFrameworkCore;
using ZeeAcom.Common.Extensions;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.PropertyModels;
using ZeeAcom.Common.Models.OwnerModels;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Properties.Queries;

public class GetPropertiesQuery : IRequest<Result<IEnumerable<PropertyModel>>>
{
    public required SearchPropertyModel Data { get; set; }
}

public class GetPropertiesQueryHandler(DatabaseContext databaseContext)
    : IRequestHandler<GetPropertiesQuery, Result<IEnumerable<PropertyModel>>>
{
    public async Task<Result<IEnumerable<PropertyModel>>> Handle(GetPropertiesQuery request, CancellationToken cancellationToken)
    {
        var search = request.Data;

        var query = databaseContext.Properties
            .AsNoTracking()
            .FilterByOwner(search.OwnerId)
            .FilterByName(search.Name)
            .FilterBySuburb(search.Suburb)
            .FilterByActive(search.Active);

        var count = await query.CountAsync(cancellationToken);

        // Project straight to the model (instead of the Mapperly mappers) so EF only selects
        // the columns needed and we don't walk the Owner -> Properties -> Owner cycle.
        var items = await query
            .OrderByDescending(e => e.DateCreated)
            .ApplyPaging(search.PagingArgs)
            .Select(e => new PropertyModel
            {
                Id = e.Id,
                Name = e.Name,
                Address = e.Address,
                Suburb = e.Suburb,
                Active = e.Active,
                MonthlyRent = e.MonthlyRent,
                DateCreated = e.DateCreated,
                SessionId = e.SessionId,
                OwnerId = e.OwnerId,
                Owner = new OwnerModel
                {
                    Id = e.Owner.Id,
                    FirstName = e.Owner.FirstName,
                    LastName = e.Owner.LastName,
                    CellPhone = e.Owner.CellPhone,
                    Email = e.Owner.Email,
                },
                PhotoCount = e.Pictures.Count,
                CoverThumbnailUrl = e.Pictures
                    .OrderBy(p => p.SortOrder).ThenBy(p => p.DateUploaded)
                    .Select(p => p.ThumbnailUrl ?? p.Url)
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<PropertyModel>>.Success(items, count);
    }
}
