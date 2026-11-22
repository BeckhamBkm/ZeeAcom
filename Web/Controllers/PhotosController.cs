using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ZeeAcom.Common;
using ZeeAcom.Common.Entities;
using ZeeAcom.Common.Models.PictureModels;
using ZeeAcom.Common.Models.PropertyModels;
using ZeeAcom.Core.Owners.Queries;
using ZeeAcom.Core.Pictures.Commands;
using ZeeAcom.Core.Properties.Queries;
using ZeeAcom.Web.Models;

namespace ZeeAcom.Web.Controllers;

/// <summary>
/// Photos of one property, for the admin (any property) and landlords (their own properties only:
/// every query/command gets the landlord's owner id as RequiredOwnerId, so other properties are "not found").
/// </summary>
[Authorize(Roles = Roles.Admin + "," + Roles.Landlord)]
[Route("Properties/{propertyId:guid}/Photos")]
public class PhotosController(IMediator mediator, UserManager<ApplicationUser> userManager) : Controller
{
    // 10 files of up to 10 MB each, plus form overhead.
    private const long MaxRequestBytes = 110L * 1024 * 1024;

    [HttpGet("")]
    public async Task<IActionResult> Index(Guid propertyId, CancellationToken cancellationToken = default)
    {
        var (allowed, ownerId) = await GetOwnerScope(cancellationToken);
        var property = allowed ? await GetProperty(propertyId, ownerId, cancellationToken) : null;
        if (property is null)
        {
            return NotFound();
        }

        return View(new PhotosPageViewModel
        {
            Property = property,
            IsAdmin = User.IsInRole(Roles.Admin),
            UploadErrors = TempData["UploadErrors"] as string[] ?? [],
        });
    }

    [HttpPost("Upload")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    public async Task<IActionResult> Upload(Guid propertyId, List<IFormFile> files, CancellationToken cancellationToken = default)
    {
        var (allowed, ownerId) = await GetOwnerScope(cancellationToken);
        if (!allowed)
        {
            return NotFound();
        }

        var result = await mediator.Send(new UploadPicturesCommand
        {
            PropertyId = propertyId,
            RequiredOwnerId = ownerId,
            Files = files.Select(f => new PictureUploadModel
            {
                FileName = Path.GetFileName(f.FileName),
                Length = f.Length,
                OpenReadStream = f.OpenReadStream,
            }).ToList(),
        }, cancellationToken);

        if (result.IsFailure && result.FirstError == "Property not found")
        {
            return NotFound();
        }

        if (result.Count is > 0)
        {
            TempData["Success"] = result.Count == 1 ? "1 photo added." : $"{result.Count} photos added.";
        }

        if (result.Errors is { Count: > 0 })
        {
            TempData["UploadErrors"] = result.Errors.ToArray();
        }

        return RedirectToAction(nameof(Index), new { propertyId });
    }

    [HttpPost("{id:guid}/Caption")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Caption(Guid propertyId, Guid id, string? caption, CancellationToken cancellationToken = default)
        => Update(propertyId, new UpdatePictureCommand { Id = id, Caption = caption ?? string.Empty }, "Caption saved.", cancellationToken);

    [HttpPost("{id:guid}/Cover")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Cover(Guid propertyId, Guid id, CancellationToken cancellationToken = default)
        => Update(propertyId, new UpdatePictureCommand { Id = id, MakeCover = true }, "Cover photo changed.", cancellationToken);

    [HttpPost("{id:guid}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid propertyId, Guid id, CancellationToken cancellationToken = default)
    {
        var (allowed, ownerId) = await GetOwnerScope(cancellationToken);
        if (!allowed || !await PictureBelongsTo(propertyId, id, ownerId, cancellationToken))
        {
            return NotFound();
        }

        var result = await mediator.Send(new DeletePictureCommand { Id = id, RequiredOwnerId = ownerId }, cancellationToken);
        if (result.IsFailure)
        {
            return NotFound();
        }

        TempData["Success"] = "Photo deleted.";
        return RedirectToAction(nameof(Index), new { propertyId });
    }

    private async Task<IActionResult> Update(Guid propertyId, UpdatePictureCommand command, string success, CancellationToken cancellationToken)
    {
        var (allowed, ownerId) = await GetOwnerScope(cancellationToken);
        if (!allowed || !await PictureBelongsTo(propertyId, command.Id, ownerId, cancellationToken))
        {
            return NotFound();
        }

        command.RequiredOwnerId = ownerId;
        var result = await mediator.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            TempData["Error"] = result.FirstError;
        }
        else
        {
            TempData["Success"] = success;
        }

        return RedirectToAction(nameof(Index), new { propertyId });
    }

    /// <summary>The photo exists, is visible to this user, and belongs to the property in the URL.</summary>
    private async Task<bool> PictureBelongsTo(Guid propertyId, Guid pictureId, Guid? ownerId, CancellationToken cancellationToken)
    {
        var property = await GetProperty(propertyId, ownerId, cancellationToken);
        return property is not null && property.Pictures.Any(p => p.Id == pictureId);
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
