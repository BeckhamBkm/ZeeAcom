using System.ComponentModel.DataAnnotations;
using ZeeAcom.Common.Entities;
using ZeeAcom.Common.Models.EnquiryModels;
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
    public IReadOnlyList<PropertyModel> Properties { get; set; } = [];
    public int EnquiryCount { get; set; }
    public int EnquiriesThisWeek { get; set; }

    /// <summary>The latest few enquiries, newest first.</summary>
    public IReadOnlyList<EnquiryModel> RecentEnquiries { get; set; } = [];

    public int LiveCount => Properties.Count(p => p.IsPublic);
    public int PhotoCount => Properties.Sum(p => p.PhotoCount);

    /// <summary>Getting-started steps; the checklist hides once they're all done.</summary>
    public IReadOnlyList<(string Title, string Hint, bool Done)> Checklist =>
    [
        ("Create your landlord account", "You're in.", true),
        ("Add your first property", "Name, address, area and monthly rent.", Properties.Count > 0),
        ("Add photos to every property", "Listings with photos get far more enquiries.", Properties.Count > 0 && Properties.All(p => p.PhotoCount > 0)),
        ("Get approved by Zee Accom", "We check new landlords before their listings go public.", Owner.Status == OwnerStatus.Approved),
        ("Go live", "Mark at least one property as available.", LiveCount > 0),
    ];

    public bool ChecklistDone => Checklist.All(c => c.Done);

    public string Greeting => DateTime.Now.Hour switch
    {
        < 12 => "Good morning",
        < 17 => "Good afternoon",
        _ => "Good evening",
    };
}
