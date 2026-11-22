using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ZeeAcom.Common;
using ZeeAcom.Common.Entities;
using ZeeAcom.Common.Models.PropertyModels;
using ZeeAcom.Core.Owners.Queries;
using ZeeAcom.Core.Properties.Queries;
using ZeeAcom.Web.Models;

namespace ZeeAcom.Web.Controllers;

/// <summary>
/// A logged-in landlord's own area. Only ever shows the signed-in landlord's data.
/// </summary>
[Authorize(Roles = Roles.Landlord)]
public class LandlordController(IMediator mediator, UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var userId = userManager.GetUserId(User)!;
        var owner = await mediator.Send(new GetOwnerByUserIdQuery { UserId = userId }, cancellationToken);
        if (owner.IsFailure || owner.Data is null)
        {
            TempData["Error"] = owner.FirstError;
            return RedirectToAction("Index", "Home");
        }

        var properties = await mediator.Send(new GetPropertiesQuery
        {
            Data = new SearchPropertyModel { OwnerId = owner.Data.Id },
        }, cancellationToken);

        return View(new LandlordDashboardViewModel
        {
            Owner = owner.Data,
            Properties = properties.Data ?? [],
        });
    }
}
