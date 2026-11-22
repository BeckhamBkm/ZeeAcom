using MediatR;
using Microsoft.EntityFrameworkCore;
using ZeeAcom.Common.Extensions;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.EntityModels;
using ZeeAcom.Common.Models.OwnerModels;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Entities.Queries;

public class GetEntitiesQuery : IRequest<Result<IEnumerable<EntityModel>>>
{
    public required SearchEntityModel Data { get; set; }
}

public class GetEntitiesQueryHandler(DatabaseContext databaseContext)
    : IRequestHandler<GetEntitiesQuery, Result<IEnumerable<EntityModel>>>
{
    public async Task<Result<IEnumerable<EntityModel>>> Handle(GetEntitiesQuery request, CancellationToken cancellationToken)
    {
        var search = request.Data;

        var query = databaseContext.Entities
            .AsNoTracking()
            .FilterByOwner(search.OwnerId)
            .FilterByName(search.Name)
            .FilterBySuburb(search.Suburb)
            .FilterByActive(search.Active);

        var count = await query.CountAsync(cancellationToken);

        // Project straight to the model (instead of the Mapperly mappers) so EF only selects
        // the columns needed and we don't walk the Owner -> Entities -> Owner cycle.
        var items = await query
            .OrderByDescending(e => e.DateCreated)
            .ApplyPaging(search.PagingArgs)
            .Select(e => new EntityModel
            {
                Id = e.Id,
                Name = e.Name,
                Address = e.Address,
                Suburb = e.Suburb,
                Active = e.Active,
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
            })
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<EntityModel>>.Success(items, count);
    }
}
