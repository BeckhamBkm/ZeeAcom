using MediatR;
using Microsoft.AspNetCore.Identity;
using ZeeAcom.Common.Entities;
using ZeeAcom.Common.Mappers;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.OwnerModels;
using ZeeAcom.Core.Notifications;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Owners.Commands;

/// <summary>
/// Admin moderation of a landlord: approve, hide or block them (or undo it by approving again).
/// Blocking also locks their login and changes its security stamp, which signs them out of any
/// open session; unblocking unlocks it. Approving a pending landlord emails them.
/// </summary>
public class SetOwnerStatusCommand : IRequest<Result<OwnerModel>>
{
    public required Guid Id { get; set; }

    public required OwnerStatus Status { get; set; }
}

public class SetOwnerStatusCommandHandler(
    DatabaseContext databaseContext,
    UserManager<ApplicationUser> userManager,
    IEmailSender emailSender) : IRequestHandler<SetOwnerStatusCommand, Result<OwnerModel>>
{
    public async Task<Result<OwnerModel>> Handle(SetOwnerStatusCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(request.Status))
        {
            return Result<OwnerModel>.Failure("Unknown status");
        }

        var owner = await databaseContext.Owners.FindAsync([request.Id], cancellationToken);
        if (owner is null)
        {
            return Result<OwnerModel>.Failure("Owner not found");
        }

        var previous = owner.Status;
        if (previous == request.Status)
        {
            return Result<OwnerModel>.Success(owner.Map());
        }

        // UserManager saves through this same DatabaseContext, so the status and the login change commit together.
        await using var transaction = await databaseContext.Database.BeginTransactionAsync(cancellationToken);

        owner.Status = request.Status;
        await databaseContext.SaveChangesAsync(cancellationToken);

        if (owner.UserId is not null && await userManager.FindByIdAsync(owner.UserId) is { } user)
        {
            var loginResult = request.Status == OwnerStatus.Blocked
                ? await Block(user)
                : previous == OwnerStatus.Blocked ? await Unblock(user) : IdentityResult.Success;

            if (!loginResult.Succeeded)
            {
                return Result<OwnerModel>.Failure(loginResult.Errors.Select(e => e.Description).ToList());
            }
        }

        await transaction.CommitAsync(cancellationToken);

        if (previous == OwnerStatus.Pending && request.Status == OwnerStatus.Approved && !string.IsNullOrWhiteSpace(owner.Email))
        {
            await emailSender.SendAsync(
                owner.Email,
                "Your Zee Accom listings are live",
                $"Hi {owner.FirstName},{Environment.NewLine}{Environment.NewLine}" +
                "Your Zee Accom landlord account has been approved. Your available properties now show on the public site, " +
                $"and people can contact you about them.{Environment.NewLine}{Environment.NewLine}Zee Accom",
                null,
                cancellationToken);
        }

        return Result<OwnerModel>.Success(owner.Map());
    }

    private async Task<IdentityResult> Block(ApplicationUser user)
    {
        var result = await userManager.SetLockoutEnabledAsync(user, true);
        if (result.Succeeded)
        {
            result = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        }

        // A new security stamp invalidates existing login cookies (checked on every request, see Program.cs).
        return result.Succeeded ? await userManager.UpdateSecurityStampAsync(user) : result;
    }

    private async Task<IdentityResult> Unblock(ApplicationUser user)
    {
        var result = await userManager.SetLockoutEndDateAsync(user, null);
        return result.Succeeded ? await userManager.ResetAccessFailedCountAsync(user) : result;
    }
}
