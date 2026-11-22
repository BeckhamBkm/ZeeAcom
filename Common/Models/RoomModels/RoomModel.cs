namespace ZeeAcom.Common.Models.RoomModels;

public sealed class RoomModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Monthly rent in rand.</summary>
    public decimal MonthlyRent { get; set; }
    public bool Available { get; set; }
    public string? Description { get; set; }
    public DateTime DateCreated { get; set; }
    public Guid PropertyId { get; set; }
}
