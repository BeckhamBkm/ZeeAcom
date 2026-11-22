using ZeeAcom.Common.Models.EnquiryModels;
using ZeeAcom.Common.Models.ListingModels;

namespace ZeeAcom.Web.Models;

public sealed class ListingsPageViewModel
{
    public string? Text { get; set; }
    public string? Suburb { get; set; }
    public decimal? MaxRent { get; set; }
    public ListingSort Sort { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; }
    public int Total { get; set; }
    public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling(Total / (double)PageSize);

    public IEnumerable<ListingModel> Listings { get; set; } = [];
    public IEnumerable<string> Suburbs { get; set; } = [];

    /// <summary>Rent ceilings offered in the "Max rent" filter.</summary>
    public static readonly int[] RentSteps = [1000, 1500, 2000, 2500, 3000, 4000, 5000, 7500, 10000];
}

public sealed class ListingDetailsPageViewModel
{
    public required ListingDetailsModel Listing { get; set; }
    public CreateEnquiryModel Enquiry { get; set; } = new();
    public string? WhatsAppUrl { get; set; }
    public string? CallUrl { get; set; }
}

public sealed class HomePageViewModel
{
    public IEnumerable<ListingModel> LatestListings { get; set; } = [];
    public IEnumerable<string> Suburbs { get; set; } = [];
    public int TotalListings { get; set; }
}
