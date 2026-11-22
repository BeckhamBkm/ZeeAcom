namespace ZeeAcom.Common.Models.ListingModels;

/// <summary>
/// A property as shown publicly in the listings grid. Contains no private owner details.
/// </summary>
public sealed class ListingModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Suburb { get; set; } = string.Empty;

    /// <summary>Monthly rent in rand; null means "rent on request".</summary>
    public decimal? MonthlyRent { get; set; }

    public string? CoverThumbnailUrl { get; set; }
    public int PhotoCount { get; set; }
    public DateTime DateCreated { get; set; }
}
