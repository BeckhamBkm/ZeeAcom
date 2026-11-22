using System.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.ListingModels;
using ZeeAcom.Core.Listings.Queries;
using ZeeAcom.Web.Models;

namespace ZeeAcom.Web.Controllers;

public class HomeController(IMediator mediator) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var latest = await mediator.Send(new SearchListingsQuery
        {
            Data = new SearchListingsModel { PagingArgs = new PagingArgs { UsePaging = true, Limit = 6 } },
        }, cancellationToken);
        var suburbs = await mediator.Send(new GetListingSuburbsQuery(), cancellationToken);

        return View(new HomePageViewModel
        {
            LatestListings = latest.Data ?? [],
            TotalListings = latest.Count ?? 0,
            Suburbs = suburbs.Data ?? [],
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
