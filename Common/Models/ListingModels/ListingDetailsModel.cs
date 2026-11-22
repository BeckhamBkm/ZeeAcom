using ZeeAcom.Common.Models.PictureModels;

namespace ZeeAcom.Common.Models.ListingModels;

/// <summary>
/// One public listing with its photos and the landlord's public contact details
/// (first name and cell phone only; the landlord's email address is never shown).
/// </summary>
public sealed class ListingDetailsModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Suburb { get; set; } = string.Empty;

    /// <summary>Monthly rent in rand; null means "rent on request".</summary>
    public decimal? MonthlyRent { get; set; }

    public DateTime DateCreated { get; set; }

    /// <summary>Photos in display order, cover first.</summary>
    public IReadOnlyList<PictureModel> Pictures { get; set; } = [];

    public string LandlordFirstName { get; set; } = string.Empty;
    public string LandlordCellPhone { get; set; } = string.Empty;
}
