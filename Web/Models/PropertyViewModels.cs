using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using ZeeAcom.Common.Models.PropertyModels;

namespace ZeeAcom.Web.Models;

public sealed class PropertyIndexViewModel
{
    public string? Name { get; set; }
    public string? Suburb { get; set; }
    public Guid? OwnerId { get; set; }
    public bool? Active { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; }
    public int Total { get; set; }
    public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling(Total / (double)PageSize);

    public IEnumerable<PropertyModel> Properties { get; set; } = [];

    [ValidateNever]
    public IEnumerable<SelectListItem> OwnerOptions { get; set; } = [];
}

/// <summary>
/// Add/edit form for a single property where the owner is already known
/// (a landlord's own property, or the admin editing an existing one).
/// </summary>
public sealed class PropertyFormViewModel
{
    /// <summary>Null when adding a new property.</summary>
    public Guid? Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(300)]
    public string Address { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Suburb { get; set; } = string.Empty;

    public bool Active { get; set; } = true;

    [Required(ErrorMessage = "Enter the monthly rent.")]
    [Range(0, 1_000_000, ErrorMessage = "Enter a monthly rent between R0 and R1 000 000.")]
    [Display(Name = "Monthly rent (R)")]
    public decimal? MonthlyRent { get; set; }

    /// <summary>Shown read-only on the admin edit page.</summary>
    [ValidateNever]
    public string? OwnerName { get; set; }

    public static PropertyFormViewModel From(PropertyModel property) => new()
    {
        Id = property.Id,
        Name = property.Name,
        Address = property.Address,
        Suburb = property.Suburb,
        Active = property.Active,
        MonthlyRent = property.MonthlyRent,
        OwnerName = property.Owner is null ? null : $"{property.Owner.FirstName} {property.Owner.LastName}",
    };

    public CreatePropertyModel ToCreateModel(Guid ownerId) => new()
    {
        Name = Name.Trim(),
        Address = Address.Trim(),
        Suburb = Suburb.Trim(),
        Active = Active,
        MonthlyRent = MonthlyRent,
        OwnerId = ownerId,
    };

    public UpdatePropertyModel ToUpdateModel() => new()
    {
        Name = Name,
        Address = Address,
        Suburb = Suburb,
        Active = Active,
        MonthlyRent = MonthlyRent,
    };
}

public sealed class PropertyCreateViewModel
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(300)]
    public string Address { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Suburb { get; set; } = string.Empty;

    public bool Active { get; set; } = true;

    [Required(ErrorMessage = "Enter the monthly rent.")]
    [Range(0, 1_000_000, ErrorMessage = "Enter a monthly rent between R0 and R1 000 000.")]
    [Display(Name = "Monthly rent (R)")]
    public decimal? MonthlyRent { get; set; }

    [Required(ErrorMessage = "Choose an owner.")]
    [Display(Name = "Owner")]
    public Guid? OwnerId { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> OwnerOptions { get; set; } = [];

    public CreatePropertyModel ToCreateModel() => new()
    {
        Name = Name.Trim(),
        Address = Address.Trim(),
        Suburb = Suburb.Trim(),
        Active = Active,
        MonthlyRent = MonthlyRent,
        OwnerId = OwnerId!.Value,
    };
}
