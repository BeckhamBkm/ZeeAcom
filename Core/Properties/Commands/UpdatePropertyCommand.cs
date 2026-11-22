using MediatR;
using Microsoft.EntityFrameworkCore;
using ZeeAcom.Common.Mappers;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.PropertyModels;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Properties.Commands;

public class UpdatePropertyCommand : IRequest<Result<PropertyModel>>
{
    public required Guid Id { get; set; }

    public required UpdatePropertyModel Data { get; set; }

    /// <summary>
    /// When set (landlord requests), only a property belonging to this owner can be changed.
    /// Someone else's property is reported as not found.
    /// </summary>
    public Guid? RequiredOwnerId { get; set; }
}

public class UpdatePropertyCommandHandler(DatabaseContext databaseContext)
    : IRequestHandler<UpdatePropertyCommand, Result<PropertyModel>>
{
    private const string Error = "Error updating a property";

    public async Task<Result<PropertyModel>> Handle(UpdatePropertyCommand request, CancellationToken cancellationToken)
    {
        var property = await databaseContext.Properties
            .Include(e => e.Owner)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (property is null || (request.RequiredOwnerId is { } ownerId && property.OwnerId != ownerId))
        {
            return Result<PropertyModel>.Failure("Property not found");
        }

        var data = request.Data;
        property.Name = data.Name.Trim();
        property.Address = data.Address.Trim();
        property.Suburb = data.Suburb.Trim();
        property.Active = data.Active;

        if (databaseContext.ChangeTracker.HasChanges())
        {
            var result = await databaseContext.SaveChangesAsync(cancellationToken);
            if (result <= 0)
            {
                return Result<PropertyModel>.Failure(Error);
            }
        }

        return Result<PropertyModel>.Success(property.Map());
    }
}
