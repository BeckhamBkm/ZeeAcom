using System.ComponentModel.DataAnnotations;

namespace ZeeAcom.Common.Models.EnquiryModels;

public sealed class CreateEnquiryModel
{
    [Required(ErrorMessage = "Enter your name."), StringLength(100)]
    [Display(Name = "Your name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter a number the landlord can reach you on."), Phone, StringLength(30)]
    [Display(Name = "Cell phone")]
    public string CellPhone { get; set; } = string.Empty;

    [EmailAddress, StringLength(200)]
    [Display(Name = "Email (optional)")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Write a short message."), StringLength(1000)]
    public string Message { get; set; } = string.Empty;
}
