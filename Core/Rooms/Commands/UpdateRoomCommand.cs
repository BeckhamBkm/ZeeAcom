using MediatR;
using Microsoft.EntityFrameworkCore;
using ZeeAcom.Common.Mappers;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.RoomModels;
using ZeeAcom.Core.Rooms.Queries;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Rooms.Commands;

public class UpdateRoomCommand : IRequest<Result<RoomModel>>
{
    public required Guid Id { get; set; }

    public required SaveRoomModel Data { get; set; }

    /// <summary>When set (landlord requests), a room in someone else's property is reported as not found.</summary>
    public Guid? RequiredOwnerId { get; set; }
}

public class UpdateRoomCommandHandler(DatabaseContext databaseContext) : IRequestHandler<UpdateRoomCommand, Result<RoomModel>>
{
    private const string Error = "Error updating a room";

    public async Task<Result<RoomModel>> Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await databaseContext.Rooms
            .FilterByOwner(request.RequiredOwnerId)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (room is null)
        {
            return Result<RoomModel>.Failure("Room not found");
        }

        request.Data.Apply(room);

        if (databaseContext.ChangeTracker.HasChanges() && await databaseContext.SaveChangesAsync(cancellationToken) <= 0)
        {
            return Result<RoomModel>.Failure(Error);
        }

        return Result<RoomModel>.Success(room.Map());
    }
}
