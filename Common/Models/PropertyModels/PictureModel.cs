namespace ZeeAcom.Common.Models.PropertyModels;

public sealed class PictureModel
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public DateTime DateUploaded { get; set; }
    public Guid PropertyId { get; set; }
}
