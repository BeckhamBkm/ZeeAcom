using MediatR;
using Microsoft.EntityFrameworkCore;
using ZeeAcom.Common.Mappers;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.OwnerModels;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Owners.Queries;

/// <summary>
/// The owner profile linked to a login (used for the landlord's own dashboard).
/// </summary>
public class GetOwnerByUserIdQuery : IRequest<Result<OwnerModel>>
{
    public required string UserId { get; set; }
}

public class GetOwnerByUserIdQueryHandler(DatabaseContext databaseContext)
    : IRequestHandler<GetOwnerByUserIdQuery, Result<OwnerModel>>
{
    public async Task<Result<OwnerModel>> Handle(GetOwnerByUserIdQuery request, CancellationToken cancellationToken)
    {
        var owner = await databaseContext.Owners
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.UserId == request.UserId, cancellationToken);

        return owner is null
            ? Result<OwnerModel>.Failure("No owner profile is linked to this account.")
            : Result<OwnerModel>.Success(owner.Map());
    }
}
