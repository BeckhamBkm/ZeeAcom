using ZeeAcom.Common.Entities;

namespace ZeeAcom.Common.Models.EntityModels;
public sealed class Entity
{
    public int Id { get; set; }

    public required string Task { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? DateCreated { get; set; }

    public Guid SessionId { get; set; }

    /// <summary>
    /// Collection Of pictures of the Entity
    /// </summary>
    public ICollection<Picture?> Pictures { get; set; } = [];

    /// <summary>
    /// Owner Navigation Property
    /// </summary>
    public Guid OwnerId { get; set; }

}
