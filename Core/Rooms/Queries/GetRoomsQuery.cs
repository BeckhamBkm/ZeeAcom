using MediatR;
using Microsoft.EntityFrameworkCore;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.RoomModels;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Rooms.Queries;

public class GetRoomsQuery : IRequest<Result<IEnumerable<RoomModel>>>
{
    public required SearchRoomModel Data { get; set; }

    /// <summary>When set (landlord requests), only rooms in this owner's properties are returned.</summary>
    public Guid? RequiredOwnerId { get; set; }
}

public class GetRoomsQueryHandler(DatabaseContext databaseContext)
    : IRequestHandler<GetRoomsQuery, Result<IEnumerable<RoomModel>>>
{
    public async Task<Result<IEnumerable<RoomModel>>> Handle(GetRoomsQuery request, CancellationToken cancellationToken)
    {
        var rooms = await databaseContext.Rooms
            .AsNoTracking()
            .FilterByOwner(request.RequiredOwnerId)
            .FilterByProperty(request.Data.PropertyId)
            .FilterByAvailable(request.Data.Available)
            .OrderBy(r => r.Name)
            .Select(r => new RoomModel
            {
                Id = r.Id,
                Name = r.Name,
                MonthlyRent = r.MonthlyRent,
                Available = r.Available,
                Description = r.Description,
                DateCreated = r.DateCreated,
                PropertyId = r.PropertyId,
            })
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<RoomModel>>.Success(rooms, rooms.Count);
    }
}
