namespace ZeeAcom.Common.Models.PictureModels;

/// <summary>One uploaded file, independent of ASP.NET's IFormFile.</summary>
public sealed class PictureUploadModel
{
    public required string FileName { get; init; }
    public required long Length { get; init; }
    public required Func<Stream> OpenReadStream { get; init; }
}
