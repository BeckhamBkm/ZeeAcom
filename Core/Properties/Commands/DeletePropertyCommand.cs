using MediatR;
using ZeeAcom.Common.Models;
using ZeeAcom.Core.Pictures;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Properties.Commands;

public class DeletePropertyCommand : IRequest<Result>
{
    public required Guid Id { get; set; }

    /// <summary>
    /// When set (landlord requests), only a property belonging to this owner can be deleted.
    /// Someone else's property is reported as not found.
    /// </summary>
    public Guid? RequiredOwnerId { get; set; }
}

public class DeletePropertyCommandHandler(DatabaseContext databaseContext, IPictureStorage storage) : IRequestHandler<DeletePropertyCommand, Result>
{
    private const string Error = "Error deleting a property record";

    public async Task<Result> Handle(DeletePropertyCommand request, CancellationToken cancellationToken)
    {
        var property = await databaseContext.Properties.FindAsync([request.Id], cancellationToken);
        if (property is null || (request.RequiredOwnerId is { } ownerId && property.OwnerId != ownerId))
        {
            return Result.Failure("Property not found");
        }

        databaseContext.Properties.Remove(property);
        var result = await databaseContext.SaveChangesAsync(cancellationToken);
        if (result <= 0)
        {
            return Result.Failure(Error);
        }

        // Picture rows go with the property (cascade); remove the photo files too.
        storage.DeleteAllForProperty(property.Id);
        return Result.Success();
    }
}
