using ZeeAcom.Common.Entities;

namespace ZeeAcom.Common.Models.EntityModels;
public sealed class EntityModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Suburb { get; set; } = string.Empty;
    public bool Active { get; set; }
    public DateTime DateCreated { get; set; }
    public Guid SessionId { get; set; }

    /// <summary>
    /// Collection Of pictures of the Entity
    /// </summary>
    public ICollection<Picture> Pictures { get; set; } = [];

    /// <summary>
    /// Owner Navigation Property
    /// </summary>
    public Guid OwnerId { get; set; }
    public Owner Owner { get; set; } = null!;
}
