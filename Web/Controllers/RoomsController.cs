using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ZeeAcom.Common;
using ZeeAcom.Common.Entities;
using ZeeAcom.Common.Models.PropertyModels;
using ZeeAcom.Common.Models.RoomModels;
using ZeeAcom.Core.Owners.Queries;
using ZeeAcom.Core.Properties.Queries;
using ZeeAcom.Core.Rooms.Commands;
using ZeeAcom.Core.Rooms.Queries;
using ZeeAcom.Web.Models;

namespace ZeeAcom.Web.Controllers;

/// <summary>
/// Rooms of one property, for both the admin (any property) and landlords (their own properties only:
/// every query/command gets the landlord's owner id as RequiredOwnerId, so other properties are "not found").
/// </summary>
[Authorize(Roles = Roles.Admin + "," + Roles.Landlord)]
[Route("Properties/{propertyId:guid}/Rooms")]
public class RoomsController(IMediator mediator, UserManager<ApplicationUser> userManager) : Controller
{
    private const string Form = "Form";

    [HttpGet("")]
    public async Task<IActionResult> Index(Guid propertyId, CancellationToken cancellationToken = default)
    {
        var (allowed, ownerId) = await GetOwnerScope(cancellationToken);
        var property = allowed ? await GetProperty(propertyId, ownerId, cancellationToken) : null;
        if (property is null)
        {
            return NotFound();
        }

        var rooms = await mediator.Send(new GetRoomsQuery
        {
            Data = new SearchRoomModel { PropertyId = propertyId },
            RequiredOwnerId = ownerId,
        }, cancellationToken);

        return View(new RoomsPageViewModel
        {
            Property = property,
            Rooms = rooms.Data ?? [],
            IsAdmin = User.IsInRole(Roles.Admin),
        });
    }

    [HttpGet("Add")]
    public async Task<IActionResult> Add(Guid propertyId, CancellationToken cancellationToken = default)
    {
        var (allowed, ownerId) = await GetOwnerScope(cancellationToken);
        var property = allowed ? await GetProperty(propertyId, ownerId, cancellationToken) : null;
        if (property is null)
        {
            return NotFound();
        }

        return View(Form, new RoomFormViewModel { PropertyId = propertyId, PropertyName = property.Name });
    }

    [HttpPost("Add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(Guid propertyId, RoomFormViewModel model, CancellationToken cancellationToken = default)
    {
        var (allowed, ownerId) = await GetOwnerScope(cancellationToken);
        var property = allowed ? await GetProperty(propertyId, ownerId, cancellationToken) : null;
        if (property is null)
        {
            return NotFound();
        }

        model.PropertyId = propertyId;
        model.PropertyName = property.Name;
        model.Id = null;
        if (!ModelState.IsValid)
        {
            return View(Form, model);
        }

        var result = await mediator.Send(new AddRoomCommand
        {
            PropertyId = propertyId,
            Data = model.Room,
            RequiredOwnerId = ownerId,
        }, cancellationToken);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.FirstError);
            return View(Form, model);
        }

        TempData["Success"] = $"{model.Room.Name} was added.";
        return RedirectToAction(nameof(Index), new { propertyId });
    }

    [HttpGet("{id:guid}/Edit")]
    public async Task<IActionResult> Edit(Guid propertyId, Guid id, CancellationToken cancellationToken = default)
    {
        var (allowed, ownerId) = await GetOwnerScope(cancellationToken);
        var property = allowed ? await GetProperty(propertyId, ownerId, cancellationToken) : null;
        var room = property is null ? null : (await mediator.Send(new GetRoomByIdQuery { Id = id, RequiredOwnerId = ownerId }, cancellationToken)).Data;
        if (property is null || room is null || room.PropertyId != propertyId)
        {
            return NotFound();
        }

        return View(Form, new RoomFormViewModel
        {
            PropertyId = propertyId,
            PropertyName = property.Name,
            Id = id,
            Room = new SaveRoomModel
            {
                Name = room.Name,
                MonthlyRent = room.MonthlyRent,
                Available = room.Available,
                Description = room.Description,
            },
        });
    }

    [HttpPost("{id:guid}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid propertyId, Guid id, RoomFormViewModel model, CancellationToken cancellationToken = default)
    {
        var (allowed, ownerId) = await GetOwnerScope(cancellationToken);
        var property = allowed ? await GetProperty(propertyId, ownerId, cancellationToken) : null;
        var room = property is null ? null : (await mediator.Send(new GetRoomByIdQuery { Id = id, RequiredOwnerId = ownerId }, cancellationToken)).Data;
        if (property is null || room is null || room.PropertyId != propertyId)
        {
            return NotFound();
        }

        model.PropertyId = propertyId;
        model.PropertyName = property.Name;
        model.Id = id;
        if (!ModelState.IsValid)
        {
            return View(Form, model);
        }

        var result = await mediator.Send(new UpdateRoomCommand { Id = id, Data = model.Room, RequiredOwnerId = ownerId }, cancellationToken);
        if (result.IsFailure)
        {
            return NotFound();
        }

        TempData["Success"] = $"{model.Room.Name} was updated.";
        return RedirectToAction(nameof(Index), new { propertyId });
    }

    [HttpPost("{id:guid}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid propertyId, Guid id, CancellationToken cancellationToken = default)
    {
        var (allowed, ownerId) = await GetOwnerScope(cancellationToken);
        var room = allowed ? (await mediator.Send(new GetRoomByIdQuery { Id = id, RequiredOwnerId = ownerId }, cancellationToken)).Data : null;
        if (room is null || room.PropertyId != propertyId)
        {
            return NotFound();
        }

        var result = await mediator.Send(new DeleteRoomCommand { Id = id, RequiredOwnerId = ownerId }, cancellationToken);
        if (result.IsFailure)
        {
            return NotFound();
        }

        TempData["Success"] = "Room deleted.";
        return RedirectToAction(nameof(Index), new { propertyId });
    }

    /// <summary>
    /// Admin: allowed, no owner restriction. Landlord: allowed only with a linked owner profile,
    /// restricted to that owner's properties.
    /// </summary>
    private async Task<(bool Allowed, Guid? OwnerId)> GetOwnerScope(CancellationToken cancellationToken)
    {
        if (User.IsInRole(Roles.Admin))
        {
            return (true, null);
        }

        var owner = await mediator.Send(new GetOwnerByUserIdQuery { UserId = userManager.GetUserId(User)! }, cancellationToken);
        return owner.Data is null ? (false, null) : (true, owner.Data.Id);
    }

    private async Task<PropertyModel?> GetProperty(Guid propertyId, Guid? ownerId, CancellationToken cancellationToken)
        => (await mediator.Send(new GetPropertyByIdQuery { Id = propertyId, RequiredOwnerId = ownerId }, cancellationToken)).Data;
}
