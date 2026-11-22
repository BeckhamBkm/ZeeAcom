using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ZeeAcom.Common;
using ZeeAcom.Common.Entities;
using ZeeAcom.Common.Models.OwnerModels;
using ZeeAcom.Common.Models.PropertyModels;
using ZeeAcom.Core.Enquiries.Queries;
using ZeeAcom.Core.Owners.Queries;
using ZeeAcom.Core.Properties.Commands;
using ZeeAcom.Core.Properties.Queries;
using ZeeAcom.Web.Models;

namespace ZeeAcom.Web.Controllers;

/// <summary>
/// A logged-in landlord's own area. Every action works on the signed-in landlord's owner profile,
/// and property commands are scoped to it (RequiredOwnerId), so a landlord can never see or change
/// someone else's property even by editing the URL.
/// </summary>
[Authorize(Roles = Roles.Landlord)]
public class LandlordController(IMediator mediator, UserManager<ApplicationUser> userManager) : Controller
{
    private const string PropertyForm = "PropertyForm";

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var owner = await GetCurrentOwner(cancellationToken);
        if (owner is null)
        {
            return NoProfile();
        }

        var properties = await mediator.Send(new GetPropertiesQuery
        {
            Data = new SearchPropertyModel { OwnerId = owner.Id },
        }, cancellationToken);
        var enquiries = await mediator.Send(new GetEnquiriesQuery { OwnerId = owner.Id }, cancellationToken);

        return View(new LandlordDashboardViewModel
        {
            Owner = owner,
            Properties = properties.Data ?? [],
            EnquiryCount = enquiries.Count ?? 0,
        });
    }

    [HttpGet]
    public async Task<IActionResult> Enquiries(CancellationToken cancellationToken = default)
    {
        var owner = await GetCurrentOwner(cancellationToken);
        if (owner is null)
        {
            return NoProfile();
        }

        var enquiries = await mediator.Send(new GetEnquiriesQuery { OwnerId = owner.Id }, cancellationToken);
        return View(enquiries.Data ?? []);
    }

    [HttpGet]
    public IActionResult AddProperty() => View(PropertyForm, new PropertyFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProperty(PropertyFormViewModel model, CancellationToken cancellationToken = default)
    {
        model.Id = null;
        if (!ModelState.IsValid)
        {
            return View(PropertyForm, model);
        }

        var owner = await GetCurrentOwner(cancellationToken);
        if (owner is null)
        {
            return NoProfile();
        }

        // The owner always comes from the login, never from the form.
        var result = await mediator.Send(new AddPropertyCommand { Data = model.ToCreateModel(owner.Id) }, cancellationToken);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.FirstError);
            return View(PropertyForm, model);
        }

        // Straight on to adding photos.
        TempData["Success"] = $"{model.Name} was added. Now add some photos.";
        return result.Data is null
            ? RedirectToAction(nameof(Index))
            : RedirectToAction("Index", "Photos", new { propertyId = result.Data.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EditProperty(Guid id, CancellationToken cancellationToken = default)
    {
        var owner = await GetCurrentOwner(cancellationToken);
        if (owner is null)
        {
            return NoProfile();
        }

        var property = await mediator.Send(new GetPropertyByIdQuery { Id = id, RequiredOwnerId = owner.Id }, cancellationToken);
        if (property.IsFailure || property.Data is null)
        {
            return NotFound();
        }

        return View(PropertyForm, PropertyFormViewModel.From(property.Data));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProperty(Guid id, PropertyFormViewModel model, CancellationToken cancellationToken = default)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            return View(PropertyForm, model);
        }

        var owner = await GetCurrentOwner(cancellationToken);
        if (owner is null)
        {
            return NoProfile();
        }

        var result = await mediator.Send(new UpdatePropertyCommand
        {
            Id = id,
            Data = model.ToUpdateModel(),
            RequiredOwnerId = owner.Id,
        }, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound();
        }

        TempData["Success"] = $"{model.Name} was updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProperty(Guid id, CancellationToken cancellationToken = default)
    {
        var owner = await GetCurrentOwner(cancellationToken);
        if (owner is null)
        {
            return NoProfile();
        }

        var result = await mediator.Send(new DeletePropertyCommand { Id = id, RequiredOwnerId = owner.Id }, cancellationToken);
        if (result.IsFailure)
        {
            return NotFound();
        }

        TempData["Success"] = "Property deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<OwnerModel?> GetCurrentOwner(CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User)!;
        var owner = await mediator.Send(new GetOwnerByUserIdQuery { UserId = userId }, cancellationToken);
        return owner.IsFailure ? null : owner.Data;
    }

    private IActionResult NoProfile()
    {
        TempData["Error"] = "No owner profile is linked to this account.";
        return RedirectToAction("Index", "Home");
    }
}
