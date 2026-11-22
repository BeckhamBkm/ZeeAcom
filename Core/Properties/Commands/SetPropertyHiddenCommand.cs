using MediatR;
using ZeeAcom.Common.Models;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Properties.Commands;

/// <summary>
/// Admin: hides one listing from the public site (or shows it again). The landlord can still see and
/// edit it, but can't make it public while it's hidden.
/// </summary>
public class SetPropertyHiddenCommand : IRequest<Result>
{
    public required Guid Id { get; set; }

    public required bool Hidden { get; set; }
}

public class SetPropertyHiddenCommandHandler(DatabaseContext databaseContext) : IRequestHandler<SetPropertyHiddenCommand, Result>
{
    public async Task<Result> Handle(SetPropertyHiddenCommand request, CancellationToken cancellationToken)
    {
        var property = await databaseContext.Properties.FindAsync([request.Id], cancellationToken);
        if (property is null)
        {
            return Result.Failure("Property not found");
        }

        if (property.HiddenByAdmin != request.Hidden)
        {
            property.HiddenByAdmin = request.Hidden;
            await databaseContext.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
