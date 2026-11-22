using MediatR;
using Microsoft.EntityFrameworkCore;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.OwnerModels;
using ZeeAcom.Common.Models.PropertyModels;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Properties.Queries;

public class GetPropertyByIdQuery : IRequest<Result<PropertyModel>>
{
    public required Guid Id { get; set; }

    /// <summary>
    /// When set (landlord requests), only a property belonging to this owner is returned.
    /// Someone else's property is reported as not found.
    /// </summary>
    public Guid? RequiredOwnerId { get; set; }
}

public class GetPropertyByIdQueryHandler(DatabaseContext databaseContext)
    : IRequestHandler<GetPropertyByIdQuery, Result<PropertyModel>>
{
    public async Task<Result<PropertyModel>> Handle(GetPropertyByIdQuery request, CancellationToken cancellationToken)
    {
        var property = await databaseContext.Properties
            .AsNoTracking()
            .Where(e => e.Id == request.Id)
            .FilterByOwner(request.RequiredOwnerId)
            .Select(e => new PropertyModel
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
            .FirstOrDefaultAsync(cancellationToken);

        return property is null
            ? Result<PropertyModel>.Failure("Property not found")
            : Result<PropertyModel>.Success(property);
    }
}
