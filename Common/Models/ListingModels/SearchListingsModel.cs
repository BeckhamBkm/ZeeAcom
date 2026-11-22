namespace ZeeAcom.Common.Models.ListingModels;

public enum ListingSort
{
    Newest,
    RentLowToHigh,
    RentHighToLow,
}

public sealed class SearchListingsModel
{
    /// <summary>Matches the property name or street address.</summary>
    public string? Text { get; set; }

    public string? Suburb { get; set; }

    /// <summary>Only listings at or below this monthly rent (listings without a rent are left out).</summary>
    public decimal? MaxRent { get; set; }

    public ListingSort Sort { get; set; } = ListingSort.Newest;

    public PagingArgs PagingArgs { get; set; } = new() { UsePaging = true };
}
