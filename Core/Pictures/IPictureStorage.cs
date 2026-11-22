namespace ZeeAcom.Core.Pictures;

/// <summary>
/// Stores property photos. Implementations check the file really is an image, make it web-friendly
/// (orientation fixed, resized, metadata such as GPS location removed) and save a full size and a thumbnail.
/// </summary>
public interface IPictureStorage
{
    /// <exception cref="InvalidPictureException">The file isn't a usable JPG, PNG or WebP image.</exception>
    Task<StoredPicture> SaveAsync(Guid propertyId, Guid pictureId, Stream content, CancellationToken cancellationToken);

    /// <summary>Deletes a stored file by its URL. URLs that aren't ours (e.g. external links) are ignored.</summary>
    void Delete(string? url);

    /// <summary>Deletes every stored photo of a property.</summary>
    void DeleteAllForProperty(Guid propertyId);
}

public sealed record StoredPicture(string Url, string ThumbnailUrl);

/// <summary>A file was rejected; the message is safe to show to the user.</summary>
public sealed class InvalidPictureException(string message) : Exception(message);
