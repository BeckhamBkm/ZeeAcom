using MediatR;
using Microsoft.AspNetCore.Authorization;
using ZeeAcom.Common;
using Microsoft.AspNetCore.Mvc;
using ZeeAcom.Common.Entities;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.PropertyModels;
using ZeeAcom.Common.Models.OwnerModels;
using ZeeAcom.Core.Owners.Commands;
using ZeeAcom.Core.Owners.Queries;
using ZeeAcom.Web.Models;

namespace ZeeAcom.Web.Controllers;

/// <summary>
/// Razor pages for owners. Uses the same MediatR handlers as Api/Controllers/OwnerController.
/// </summary>
[Authorize(Roles = Roles.Admin)]
public class OwnersController(IMediator mediator) : Controller
{
    private const int PageSize = 20;

    [HttpGet]
    public async Task<IActionResult> Index(string? firstName, string? lastName, OwnerStatus? status, int page = 1,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);

        var result = await mediator.Send(new GetOwnersQuery
        {
            Data = new SearchOwnerModel
            {
                FirstName = firstName,
                LastName = lastName,
                Status = status,
                PagingArgs = new PagingArgs { UsePaging = true, Limit = PageSize, Offset = (page - 1) * PageSize },
            },
        }, cancellationToken);

        if (result.IsFailure)
        {
            TempData["Error"] = result.FirstError;
        }

        var pending = await mediator.Send(new GetOwnersQuery
        {
            Data = new SearchOwnerModel
            {
                Status = OwnerStatus.Pending,
                PagingArgs = new PagingArgs { UsePaging = true, Limit = 1 },
            },
        }, cancellationToken);

        return View(new OwnerIndexViewModel
        {
            FirstName = firstName,
            LastName = lastName,
            Status = status,
            PendingCount = pending.Count ?? 0,
            Page = page,
            PageSize = PageSize,
            Total = result.Count ?? 0,
            Owners = result.Data ?? [],
        });
    }

    /// <summary>Approve, hide or block a landlord (approving again undoes hide/block).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(Guid id, OwnerStatus status, OwnerStatus? filter, CancellationToken cancellationToken = default)
    {
        // A value that doesn't bind would otherwise fall back to the default status (Pending).
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var result = await mediator.Send(new SetOwnerStatusCommand { Id = id, Status = status }, cancellationToken);
        if (result.IsFailure || result.Data is null)
        {
            TempData["Error"] = result.FirstError;
        }
        else
        {
            var name = $"{result.Data.FirstName} {result.Data.LastName}";
            TempData["Success"] = status switch
            {
                OwnerStatus.Approved => $"{name} is approved; their available properties are public.",
                OwnerStatus.Hidden => $"{name}'s listings are hidden. They can still log in.",
                OwnerStatus.Blocked => $"{name} is blocked: logged out, can't log in, and their listings are hidden.",
                _ => $"{name} is waiting for approval.",
            };
        }

        return RedirectToAction(nameof(Index), new { status = filter });
    }

    [HttpGet]
    public IActionResult Create() => View(new CreateOwnerModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateOwnerModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await mediator.Send(new AddOwnerCommand { Data = model }, cancellationToken);
        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.FirstError);
            return View(model);
        }

        TempData["Success"] = $"{model.FirstName} {model.LastName} was added.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new DeleteOwnerCommand { Id = id }, cancellationToken);
        if (result.IsFailure)
        {
            TempData["Error"] = result.FirstError;
        }
        else
        {
            TempData["Success"] = "Owner deleted.";
        }

        return RedirectToAction(nameof(Index));
    }
}
