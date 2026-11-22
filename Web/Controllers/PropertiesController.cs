using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.EntityModels;
using ZeeAcom.Core.Entities.Commands;
using ZeeAcom.Core.Entities.Queries;
using ZeeAcom.Core.Owners.Queries;
using ZeeAcom.Web.Models;

namespace ZeeAcom.Web.Controllers;

/// <summary>
/// Razor pages for properties (the `Entity` table). Uses the same MediatR handlers as Api/Controllers/EntityController.
/// </summary>
public class PropertiesController(IMediator mediator) : Controller
{
    private const int PageSize = 20;

    [HttpGet]
    public async Task<IActionResult> Index(string? name, string? suburb, Guid? ownerId, bool? active, int page = 1,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);

        var result = await mediator.Send(new GetEntitiesQuery
        {
            Data = new SearchEntityModel
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

        var result = await mediator.Send(new AddEntityCommand { Data = model.ToCreateModel() }, cancellationToken);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.FirstError);
            model.OwnerOptions = await GetOwnerOptions(model.OwnerId, cancellationToken);
            return View(model);
        }

        TempData["Success"] = $"{model.Name} was added.";
        return RedirectToAction(nameof(Index), new { ownerId = model.OwnerId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, Guid? ownerId, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new DeleteEntityCommand { Id = id }, cancellationToken);
        TempData[result.IsFailure ? "Error" : "Success"] = result.IsFailure ? result.FirstError : "Property deleted.";

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
