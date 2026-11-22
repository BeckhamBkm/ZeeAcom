using MediatR;
using Microsoft.EntityFrameworkCore;
using ZeeAcom.Common.Mappers;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.RoomModels;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Rooms.Queries;

public class GetRoomByIdQuery : IRequest<Result<RoomModel>>
{
    public required Guid Id { get; set; }

    /// <summary>When set (landlord requests), a room in someone else's property is reported as not found.</summary>
    public Guid? RequiredOwnerId { get; set; }
}

public class GetRoomByIdQueryHandler(DatabaseContext databaseContext) : IRequestHandler<GetRoomByIdQuery, Result<RoomModel>>
{
    public async Task<Result<RoomModel>> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
    {
        var room = await databaseContext.Rooms
            .AsNoTracking()
            .FilterByOwner(request.RequiredOwnerId)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        return room is null
            ? Result<RoomModel>.Failure("Room not found")
            : Result<RoomModel>.Success(room.Map());
    }
}
