using System.ComponentModel.DataAnnotations;

namespace ZeeAcom.Common.Models.RoomModels;

/// <summary>
/// Fields for adding or editing a room. The property is set separately and can't be changed.
/// </summary>
public sealed class SaveRoomModel
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(0, 1_000_000, ErrorMessage = "Enter a monthly rent between R0 and R1 000 000.")]
    [Display(Name = "Monthly rent (R)")]
    public decimal MonthlyRent { get; set; }

    public bool Available { get; set; } = true;

    [StringLength(1000)]
    public string? Description { get; set; }
}
