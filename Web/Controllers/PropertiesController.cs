using MediatR;
using Microsoft.AspNetCore.Authorization;
using ZeeAcom.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.OwnerModels;
using ZeeAcom.Common.Models.PropertyModels;
using ZeeAcom.Core.Properties.Commands;
using ZeeAcom.Core.Properties.Queries;
using ZeeAcom.Core.Owners.Queries;
using ZeeAcom.Web.Models;

namespace ZeeAcom.Web.Controllers;

/// <summary>
/// Razor pages for properties. Uses the same MediatR handlers as Api/Controllers/PropertyController.
/// </summary>
[Authorize(Roles = Roles.Admin)]
public class PropertiesController(IMediator mediator) : Controller
{
    private const int PageSize = 20;

    [HttpGet]
    public async Task<IActionResult> Index(string? name, string? suburb, Guid? ownerId, bool? active, int page = 1,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);

        var result = await mediator.Send(new GetPropertiesQuery
        {
            Data = new SearchPropertyModel
            {
                Name = name,
                Suburb = suburb,
                OwnerId = ownerId,
                Active = active,
                PagingArgs = new PagingArgs { UsePaging = true, Limit = PageSize, Offset = (page - 1) * PageSize },
            },
        }, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.FirstError;
        }

        return View(new PropertyIndexViewModel
        {
            Name = name,
            Suburb = suburb,
            OwnerId = ownerId,
            Active = active,
            Page = page,
            PageSize = PageSize,
            Total = result.Count ?? 0,
            Properties = result.Data ?? [],
            OwnerOptions = await GetOwnerOptions(ownerId, cancellationToken),
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create(Guid? ownerId, CancellationToken cancellationToken = default)
        => View(new PropertyCreateViewModel
        {
            OwnerId = ownerId,
            OwnerOptions = await GetOwnerOptions(ownerId, cancellationToken),
        });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PropertyCreateViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            model.OwnerOptions = await GetOwnerOptions(model.OwnerId, cancellationToken);
            return View(model);
        }

        var result = await mediator.Send(new AddPropertyCommand { Data = model.ToCreateModel() }, cancellationToken);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.FirstError);
            model.OwnerOptions = await GetOwnerOptions(model.OwnerId, cancellationToken);
            return View(model);
        }

        TempData["Success"] = $"{model.Name} was added.";
        return RedirectToAction(nameof(Index), new { ownerId = model.OwnerId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken = default)
    {
        var property = await mediator.Send(new GetPropertyByIdQuery { Id = id }, cancellationToken);
        if (property.IsFailure || property.Data is null)
        {
            return NotFound();
        }

        return View(PropertyFormViewModel.From(property.Data));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, PropertyFormViewModel model, CancellationToken cancellationToken = default)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await mediator.Send(new UpdatePropertyCommand { Id = id, Data = model.ToUpdateModel() }, cancellationToken);
        if (result.IsFailure || result.Data is null)
        {
            return NotFound();
        }

        TempData["Success"] = $"{model.Name} was updated.";
        return RedirectToAction(nameof(Index), new { ownerId = result.Data.OwnerId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, Guid? ownerId, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new DeletePropertyCommand { Id = id }, cancellationToken);
        TempData[result.IsFailure ? "Error" : "Success"] = result.IsFailure ? result.FirstError : "Property deleted.";

        return RedirectToAction(nameof(Index), new { ownerId });
    }

    /// <summary>Hide one listing from the public site, or show it again.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetHidden(Guid id, bool hidden, Guid? ownerId, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var result = await mediator.Send(new SetPropertyHiddenCommand { Id = id, Hidden = hidden }, cancellationToken);
        TempData[result.IsFailure ? "Error" : "Success"] = result.IsFailure
            ? result.FirstError
            : hidden ? "Listing hidden from the public site." : "Listing no longer hidden.";

        return RedirectToAction(nameof(Index), new { ownerId });
    }

    private async Task<IEnumerable<SelectListItem>> GetOwnerOptions(Guid? selected, CancellationToken cancellationToken)
    {
        var owners = await mediator.Send(new GetOwnersQuery { Data = new SearchOwnerModel() }, cancellationToken);

        return (owners.Data ?? [])
            .OrderBy(o => o.LastName).ThenBy(o => o.FirstName)
            .Select(o => new SelectListItem($"{o.FirstName} {o.LastName}", o.Id.ToString(), o.Id == selected))
            .ToList();
    }
}
