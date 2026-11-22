using MediatR;
using Microsoft.EntityFrameworkCore;
using ZeeAcom.Common.Models;
using ZeeAcom.Core.Rooms.Queries;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Rooms.Commands;

public class DeleteRoomCommand : IRequest<Result>
{
    public required Guid Id { get; set; }

    /// <summary>When set (landlord requests), a room in someone else's property is reported as not found.</summary>
    public Guid? RequiredOwnerId { get; set; }
}

public class DeleteRoomCommandHandler(DatabaseContext databaseContext) : IRequestHandler<DeleteRoomCommand, Result>
{
    private const string Error = "Error deleting a room";

    public async Task<Result> Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
    {
        var room = await databaseContext.Rooms
            .FilterByOwner(request.RequiredOwnerId)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (room is null)
        {
            return Result.Failure("Room not found");
        }

        databaseContext.Rooms.Remove(room);
        return await databaseContext.SaveChangesAsync(cancellationToken) > 0 ? Result.Success() : Result.Failure(Error);
    }
}
