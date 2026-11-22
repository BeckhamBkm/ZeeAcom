using MediatR;
using Microsoft.EntityFrameworkCore;
using ZeeAcom.Common.Models;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Pictures.Commands;

/// <summary>Changes a photo's caption and/or makes it the property's cover photo.</summary>
public class UpdatePictureCommand : IRequest<Result>
{
    public required Guid Id { get; set; }

    /// <summary>New caption; null leaves it unchanged, empty clears it.</summary>
    public string? Caption { get; set; }

    /// <summary>Move this photo to the front, making it the cover photo shown in listings.</summary>
    public bool MakeCover { get; set; }

    /// <summary>When set (landlord requests), a photo of someone else's property is reported as not found.</summary>
    public Guid? RequiredOwnerId { get; set; }
}

public class UpdatePictureCommandHandler(DatabaseContext databaseContext) : IRequestHandler<UpdatePictureCommand, Result>
{
    public async Task<Result> Handle(UpdatePictureCommand request, CancellationToken cancellationToken)
    {
        var picture = await databaseContext.Pictures
            .FirstOrDefaultAsync(p => p.Id == request.Id
                && (request.RequiredOwnerId == null || p.Property.OwnerId == request.RequiredOwnerId), cancellationToken);

        if (picture is null)
        {
            return Result.Failure("Photo not found");
        }

        if (request.Caption is not null)
        {
            var caption = request.Caption.Trim();
            if (caption.Length > 100)
            {
                return Result.Failure("Keep the caption under 100 characters.");
            }

            picture.Caption = caption.Length == 0 ? null : caption;
        }

        if (request.MakeCover)
        {
            var lowest = await databaseContext.Pictures
                .Where(p => p.PropertyId == picture.PropertyId)
                .MinAsync(p => p.SortOrder, cancellationToken);
            picture.SortOrder = lowest - 1;
        }

        if (databaseContext.ChangeTracker.HasChanges())
        {
            await databaseContext.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
