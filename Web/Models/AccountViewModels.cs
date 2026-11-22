using System.ComponentModel.DataAnnotations;
using ZeeAcom.Common.Models.OwnerModels;
using ZeeAcom.Common.Models.PropertyModels;

namespace ZeeAcom.Web.Models;

public sealed class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Keep me logged in")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public sealed class LandlordDashboardViewModel
{
    public required OwnerModel Owner { get; set; }
    public IEnumerable<PropertyModel> Properties { get; set; } = [];
    public int EnquiryCount { get; set; }
}
