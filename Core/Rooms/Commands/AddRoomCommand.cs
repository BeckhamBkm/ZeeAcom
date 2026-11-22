using MediatR;
using Microsoft.EntityFrameworkCore;
using ZeeAcom.Common.Entities;
using ZeeAcom.Common.Mappers;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.RoomModels;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Rooms.Commands;

public class AddRoomCommand : IRequest<Result<RoomModel>>
{
    public required Guid PropertyId { get; set; }

    public required SaveRoomModel Data { get; set; }

    /// <summary>When set (landlord requests), the property must belong to this owner.</summary>
    public Guid? RequiredOwnerId { get; set; }
}

public class AddRoomCommandHandler(DatabaseContext databaseContext) : IRequestHandler<AddRoomCommand, Result<RoomModel>>
{
    private const string Error = "Error adding a room";

    public async Task<Result<RoomModel>> Handle(AddRoomCommand request, CancellationToken cancellationToken)
    {
        var propertyExists = await databaseContext.Properties.AnyAsync(
            p => p.Id == request.PropertyId && (request.RequiredOwnerId == null || p.OwnerId == request.RequiredOwnerId),
            cancellationToken);

        if (!propertyExists)
        {
            return Result<RoomModel>.Failure("Property not found");
        }

        var room = new Room
        {
            PropertyId = request.PropertyId,
            DateCreated = DateTime.UtcNow,
        };
        request.Data.Apply(room);

        databaseContext.Rooms.Add(room);
        var result = await databaseContext.SaveChangesAsync(cancellationToken);

        return result > 0 ? Result<RoomModel>.Success(room.Map()) : Result<RoomModel>.Failure(Error);
    }
}
