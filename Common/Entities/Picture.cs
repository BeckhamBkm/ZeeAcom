namespace ZeeAcom.Common.Entities;

public partial class Picture
{
    public Guid Id { get; set; }

    public string Url { get; set; } = null!;

    public DateTime DateUploaded { get; set; }

    public Guid PropertyId { get; set; }

    public virtual Property Property { get; set; } = null!;
}
