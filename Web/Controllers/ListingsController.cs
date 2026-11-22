using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ZeeAcom.Common;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.EnquiryModels;
using ZeeAcom.Common.Models.ListingModels;
using ZeeAcom.Core.Enquiries.Commands;
using ZeeAcom.Core.Listings.Queries;
using ZeeAcom.Web.Helpers;
using ZeeAcom.Web.Models;

namespace ZeeAcom.Web.Controllers;

/// <summary>
/// The public site: browse available properties and contact the landlord. No login needed.
/// </summary>
[AllowAnonymous]
[Route("Listings")]
public class ListingsController(IMediator mediator) : Controller
{
    private const int PageSize = 12;

    [HttpGet("")]
    public async Task<IActionResult> Index(string? text, string? suburb, decimal? maxRent, ListingSort sort = ListingSort.Newest,
        int page = 1, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        var result = await mediator.Send(new SearchListingsQuery
        {
            Data = new SearchListingsModel
            {
                Text = text,
                Suburb = suburb,
                MaxRent = maxRent,
                Sort = sort,
                PagingArgs = new PagingArgs { UsePaging = true, Limit = PageSize, Offset = (page - 1) * PageSize },
            },
        }, cancellationToken);

        var suburbs = await mediator.Send(new GetListingSuburbsQuery(), cancellationToken);

        return View(new ListingsPageViewModel
        {
            Text = text,
            Suburb = suburb,
            MaxRent = maxRent,
            Sort = sort,
            Page = page,
            PageSize = PageSize,
            Total = result.Count ?? 0,
            Listings = result.Data ?? [],
            Suburbs = suburbs.Data ?? [],
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken = default)
    {
        var listing = (await mediator.Send(new GetListingQuery { Id = id }, cancellationToken)).Data;
        return listing is null ? NotFound() : View(BuildDetailsPage(listing, new CreateEnquiryModel()));
    }

    [HttpPost("{id:guid}/Enquire")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(RateLimits.Enquiries)]
    public async Task<IActionResult> Enquire(Guid id, [Bind(Prefix = "Enquiry")] CreateEnquiryModel enquiry, string? website,
        CancellationToken cancellationToken = default)
    {
        var listing = (await mediator.Send(new GetListingQuery { Id = id }, cancellationToken)).Data;
        if (listing is null)
        {
            return NotFound();
        }

        // "website" is a hidden honeypot field people never see; bots that fill it in are quietly ignored.
        if (!string.IsNullOrEmpty(website))
        {
            TempData["EnquirySent"] = true;
            return RedirectToAction(nameof(Details), new { id });
        }

        if (!ModelState.IsValid)
        {
            return View(nameof(Details), BuildDetailsPage(listing, enquiry));
        }

        var result = await mediator.Send(new SendEnquiryCommand { PropertyId = id, Data = enquiry }, cancellationToken);
        if (result.IsFailure)
        {
            return NotFound();
        }

        TempData["EnquirySent"] = true;
        return RedirectToAction(nameof(Details), new { id });
    }

    private static ListingDetailsPageViewModel BuildDetailsPage(ListingDetailsModel listing, CreateEnquiryModel enquiry) => new()
    {
        Listing = listing,
        Enquiry = enquiry,
        CallUrl = PhoneLinks.Tel(listing.LandlordCellPhone),
        WhatsAppUrl = PhoneLinks.WhatsApp(listing.LandlordCellPhone,
            $"Hi {listing.LandlordFirstName}, I saw {listing.Name} ({listing.Suburb}) on Zee Accom. Is it still available?"),
    };
}
