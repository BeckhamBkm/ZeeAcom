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

public sealed class PropertyCreateViewModel
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(300)]
    public string Address { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Suburb { get; set; } = string.Empty;

    public bool Active { get; set; } = true;

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
        OwnerId = OwnerId!.Value,
    };
}
