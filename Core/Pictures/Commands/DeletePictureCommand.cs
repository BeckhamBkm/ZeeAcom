using MediatR;
using Microsoft.EntityFrameworkCore;
using ZeeAcom.Common.Models;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Pictures.Commands;

public class DeletePictureCommand : IRequest<Result>
{
    public required Guid Id { get; set; }

    /// <summary>When set (landlord requests), a photo of someone else's property is reported as not found.</summary>
    public Guid? RequiredOwnerId { get; set; }
}

public class DeletePictureCommandHandler(DatabaseContext databaseContext, IPictureStorage storage)
    : IRequestHandler<DeletePictureCommand, Result>
{
    public async Task<Result> Handle(DeletePictureCommand request, CancellationToken cancellationToken)
    {
        var picture = await databaseContext.Pictures
            .FirstOrDefaultAsync(p => p.Id == request.Id
                && (request.RequiredOwnerId == null || p.Property.OwnerId == request.RequiredOwnerId), cancellationToken);

        if (picture is null)
        {
            return Result.Failure("Photo not found");
        }

        databaseContext.Pictures.Remove(picture);
        if (await databaseContext.SaveChangesAsync(cancellationToken) <= 0)
        {
            return Result.Failure("Error deleting the photo");
        }

        storage.Delete(picture.Url);
        storage.Delete(picture.ThumbnailUrl);
        return Result.Success();
    }
}
