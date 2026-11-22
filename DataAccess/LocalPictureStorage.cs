using SkiaSharp;
using ZeeAcom.Core.Pictures;

namespace ZeeAcom.DataAccess;

/// <summary>
/// Saves property photos under wwwroot/uploads/properties/{propertyId}/ as
/// {pictureId}.jpg (longest side max 1600px) and {pictureId}_thumb.jpg (max 480px).
/// Images are re-encoded, so EXIF metadata (camera details, GPS location) is never stored.
/// </summary>
public sealed class LocalPictureStorage(IWebHostEnvironment environment, ILogger<LocalPictureStorage> logger) : IPictureStorage
{
    private const string UrlRoot = "/uploads/properties";
    private const int FullSize = 1600;
    private const int ThumbnailSize = 480;
    private const long MaxPixels = 50_000_000; // refuse absurdly large images (e.g. decompression bombs)

    private string UploadsRoot => Path.Combine(environment.WebRootPath, "uploads", "properties");

    public async Task<StoredPicture> SaveAsync(Guid propertyId, Guid pictureId, Stream content, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;

        using var codec = SKCodec.Create(buffer)
            ?? throw new InvalidPictureException("isn't a JPG, PNG or WebP image.");

        if (codec.EncodedFormat is not (SKEncodedImageFormat.Jpeg or SKEncodedImageFormat.Png or SKEncodedImageFormat.Webp))
        {
            throw new InvalidPictureException("isn't a JPG, PNG or WebP image.");
        }

        if ((long)codec.Info.Width * codec.Info.Height > MaxPixels)
        {
            throw new InvalidPictureException("is too large (over 50 megapixels).");
        }

        using var decoded = SKBitmap.Decode(codec)
            ?? throw new InvalidPictureException("couldn't be read; the file may be damaged.");
        using var upright = AutoOrient(decoded, codec.EncodedOrigin);

        var folder = Path.Combine(UploadsRoot, propertyId.ToString());
        Directory.CreateDirectory(folder);

        var fullName = $"{pictureId}.jpg";
        var thumbName = $"{pictureId}_thumb.jpg";
        await File.WriteAllBytesAsync(Path.Combine(folder, fullName), EncodeJpeg(upright, FullSize, 82), cancellationToken);
        await File.WriteAllBytesAsync(Path.Combine(folder, thumbName), EncodeJpeg(upright, ThumbnailSize, 75), cancellationToken);

        return new StoredPicture($"{UrlRoot}/{propertyId}/{fullName}", $"{UrlRoot}/{propertyId}/{thumbName}");
    }

    public void Delete(string? url)
    {
        var path = ToLocalPath(url);
        if (path is null)
        {
            return;
        }

        try
        {
            File.Delete(path);
        }
        catch (IOException ex)
        {
            logger.LogWarning(ex, "Couldn't delete picture file {Path}", path);
        }
    }

    public void DeleteAllForProperty(Guid propertyId)
    {
        var folder = Path.Combine(UploadsRoot, propertyId.ToString());
        try
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, recursive: true);
            }
        }
        catch (IOException ex)
        {
            logger.LogWarning(ex, "Couldn't delete picture folder {Folder}", folder);
        }
    }

    /// <summary>Maps one of our URLs to its file, refusing anything outside the uploads folder.</summary>
    private string? ToLocalPath(string? url)
    {
        if (string.IsNullOrEmpty(url) || !url.StartsWith(UrlRoot + "/", StringComparison.Ordinal))
        {
            return null;
        }

        var root = Path.GetFullPath(UploadsRoot) + Path.DirectorySeparatorChar;
        var path = Path.GetFullPath(Path.Combine(UploadsRoot, url[(UrlRoot.Length + 1)..].Replace('/', Path.DirectorySeparatorChar)));
        return path.StartsWith(root, StringComparison.OrdinalIgnoreCase) ? path : null;
    }

    /// <summary>Applies the camera's EXIF rotation so the photo is upright once the metadata is gone.</summary>
    private static SKBitmap AutoOrient(SKBitmap source, SKEncodedOrigin origin)
    {
        SKBitmap rotated;
        switch (origin)
        {
            case SKEncodedOrigin.BottomRight: // 180°
                rotated = new SKBitmap(source.Width, source.Height);
                using (var canvas = new SKCanvas(rotated))
                {
                    canvas.RotateDegrees(180, source.Width / 2f, source.Height / 2f);
                    canvas.DrawBitmap(source, 0, 0);
                }
                return rotated;

            case SKEncodedOrigin.RightTop: // 90° clockwise (typical portrait phone photo)
                rotated = new SKBitmap(source.Height, source.Width);
                using (var canvas = new SKCanvas(rotated))
                {
                    canvas.Translate(rotated.Width, 0);
                    canvas.RotateDegrees(90);
                    canvas.DrawBitmap(source, 0, 0);
                }
                return rotated;

            case SKEncodedOrigin.LeftBottom: // 90° anticlockwise
                rotated = new SKBitmap(source.Height, source.Width);
                using (var canvas = new SKCanvas(rotated))
                {
                    canvas.Translate(0, rotated.Height);
                    canvas.RotateDegrees(270);
                    canvas.DrawBitmap(source, 0, 0);
                }
                return rotated;

            default: // already upright (mirrored orientations are rare and left as-is)
                return source.Copy();
        }
    }

    /// <summary>Scales down to fit <paramref name="maxSide"/> (never up), flattens transparency onto white, encodes as JPEG.</summary>
    private static byte[] EncodeJpeg(SKBitmap source, int maxSide, int quality)
    {
        var scale = Math.Min(1.0, (double)maxSide / Math.Max(source.Width, source.Height));
        var width = Math.Max(1, (int)Math.Round(source.Width * scale));
        var height = Math.Max(1, (int)Math.Round(source.Height * scale));

        using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        surface.Canvas.Clear(SKColors.White);
        using (var image = SKImage.FromBitmap(source))
        {
            surface.Canvas.DrawImage(image, new SKRect(0, 0, width, height), new SKSamplingOptions(SKCubicResampler.Mitchell));
        }

        using var snapshot = surface.Snapshot();
        using var data = snapshot.Encode(SKEncodedImageFormat.Jpeg, quality);
        return data.ToArray();
    }
}
