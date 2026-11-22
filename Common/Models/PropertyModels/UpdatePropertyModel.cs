using System.ComponentModel.DataAnnotations;

namespace ZeeAcom.Common.Models.PropertyModels;

/// <summary>
/// The fields that can be changed on an existing property. The owner can't be changed here.
/// </summary>
public sealed class UpdatePropertyModel
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(300)]
    public string Address { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Suburb { get; set; } = string.Empty;

    public bool Active { get; set; }

    /// <summary>Monthly rent in rand.</summary>
    [Range(0, 1_000_000, ErrorMessage = "Enter a monthly rent between R0 and R1 000 000.")]
    public decimal? MonthlyRent { get; set; }
}
