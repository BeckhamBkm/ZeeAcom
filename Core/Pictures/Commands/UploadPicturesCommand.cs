using MediatR;
using Microsoft.EntityFrameworkCore;
using ZeeAcom.Common.Entities;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.PictureModels;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Pictures.Commands;

/// <summary>
/// Adds photos to a property. Files that aren't usable images are skipped and reported in
/// <see cref="Result{T}.Errors"/>; the rest are still saved.
/// </summary>
public class UploadPicturesCommand : IRequest<Result<IEnumerable<PictureModel>>>
{
    public const int MaxPhotosPerProperty = 20;
    public const int MaxFilesPerUpload = 10;
    public const long MaxFileBytes = 10 * 1024 * 1024;

    public required Guid PropertyId { get; set; }

    public required IReadOnlyList<PictureUploadModel> Files { get; set; }

    /// <summary>When set (landlord requests), the property must belong to this owner.</summary>
    public Guid? RequiredOwnerId { get; set; }
}

public class UploadPicturesCommandHandler(DatabaseContext databaseContext, IPictureStorage storage)
    : IRequestHandler<UploadPicturesCommand, Result<IEnumerable<PictureModel>>>
{
    public async Task<Result<IEnumerable<PictureModel>>> Handle(UploadPicturesCommand request, CancellationToken cancellationToken)
    {
        var property = await databaseContext.Properties
            .Where(p => p.Id == request.PropertyId && (request.RequiredOwnerId == null || p.OwnerId == request.RequiredOwnerId))
            .Select(p => new { p.Id, Count = p.Pictures.Count, MaxOrder = p.Pictures.Max(x => (int?)x.SortOrder) })
            .FirstOrDefaultAsync(cancellationToken);

        if (property is null)
        {
            return Result<IEnumerable<PictureModel>>.Failure("Property not found");
        }

        if (request.Files.Count == 0)
        {
            return Result<IEnumerable<PictureModel>>.Failure("Choose at least one photo.");
        }

        if (request.Files.Count > UploadPicturesCommand.MaxFilesPerUpload)
        {
            return Result<IEnumerable<PictureModel>>.Failure($"Upload at most {UploadPicturesCommand.MaxFilesPerUpload} photos at a time.");
        }

        var room = UploadPicturesCommand.MaxPhotosPerProperty - property.Count;
        if (room <= 0)
        {
            return Result<IEnumerable<PictureModel>>.Failure($"This property already has the maximum of {UploadPicturesCommand.MaxPhotosPerProperty} photos.");
        }

        var errors = new List<string>();
        var saved = new List<Picture>();
        var nextOrder = (property.MaxOrder ?? -1) + 1;

        foreach (var file in request.Files)
        {
            if (saved.Count >= room)
            {
                errors.Add($"{file.FileName}: skipped, the property reached the maximum of {UploadPicturesCommand.MaxPhotosPerProperty} photos.");
                continue;
            }

            if (file.Length > UploadPicturesCommand.MaxFileBytes)
            {
                errors.Add($"{file.FileName}: is larger than 10 MB.");
                continue;
            }

            var picture = new Picture
            {
                Id = Guid.NewGuid(),
                PropertyId = property.Id,
                DateUploaded = DateTime.UtcNow,
                SortOrder = nextOrder,
            };

            try
            {
                await using var stream = file.OpenReadStream();
                var stored = await storage.SaveAsync(property.Id, picture.Id, stream, cancellationToken);
                picture.Url = stored.Url;
                picture.ThumbnailUrl = stored.ThumbnailUrl;
            }
            catch (InvalidPictureException ex)
            {
                errors.Add($"{file.FileName}: {ex.Message}");
                continue;
            }

            saved.Add(picture);
            nextOrder++;
        }

        if (saved.Count == 0)
        {
            return Result<IEnumerable<PictureModel>>.Failure(errors);
        }

        databaseContext.Pictures.AddRange(saved);
        try
        {
            await databaseContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Don't leave files behind for rows that were never saved.
            foreach (var picture in saved)
            {
                storage.Delete(picture.Url);
                storage.Delete(picture.ThumbnailUrl);
            }

            throw;
        }

        var result = Result<IEnumerable<PictureModel>>.Success(saved.Select(p => new PictureModel
        {
            Id = p.Id,
            Url = p.Url,
            ThumbnailUrl = p.ThumbnailUrl,
            SortOrder = p.SortOrder,
            DateUploaded = p.DateUploaded,
            PropertyId = p.PropertyId,
        }).ToList(), saved.Count);
        result.Errors = errors.Count > 0 ? errors : null;
        return result;
    }
}
