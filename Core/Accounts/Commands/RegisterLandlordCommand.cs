using MediatR;
using Microsoft.AspNetCore.Identity;
using ZeeAcom.Common;
using ZeeAcom.Common.Entities;
using ZeeAcom.Common.Mappers;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.AccountModels;
using ZeeAcom.Common.Models.OwnerModels;
using ZeeAcom.Core.Notifications;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Accounts.Commands;

/// <summary>
/// Creates a landlord login (Landlord role) and its linked owner profile in one transaction.
/// The landlord starts as Pending (not public) and the admin is emailed to approve them.
/// </summary>
public class RegisterLandlordCommand : IRequest<Result<OwnerModel>>
{
    public required RegisterLandlordModel Data { get; set; }
}

public class RegisterLandlordCommandHandler(
    DatabaseContext databaseContext,
    UserManager<ApplicationUser> userManager,
    IEmailSender emailSender,
    IConfiguration configuration)
    : IRequestHandler<RegisterLandlordCommand, Result<OwnerModel>>
{
    public async Task<Result<OwnerModel>> Handle(RegisterLandlordCommand request, CancellationToken cancellationToken)
    {
        var data = request.Data;
        var email = data.Email.Trim();

        // UserManager saves through this same DatabaseContext, so the user, role and owner commit (or roll back) together.
        await using var transaction = await databaseContext.Database.BeginTransactionAsync(cancellationToken);

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            PhoneNumber = data.CellPhone.Trim(),
        };

        var created = await userManager.CreateAsync(user, data.Password);
        if (!created.Succeeded)
        {
            return Result<OwnerModel>.Failure(created.Errors.Select(e => e.Description).ToList());
        }

        var addedToRole = await userManager.AddToRoleAsync(user, Roles.Landlord);
        if (!addedToRole.Succeeded)
        {
            return Result<OwnerModel>.Failure(addedToRole.Errors.Select(e => e.Description).ToList());
        }

        var owner = new Owner
        {
            FirstName = data.FirstName.Trim(),
            LastName = data.LastName.Trim(),
            CellPhone = data.CellPhone.Trim(),
            Email = email,
            UserId = user.Id,
            Status = OwnerStatus.Pending,
        };

        databaseContext.Owners.Add(owner);
        await databaseContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        if (configuration["AdminUser:Email"] is { Length: > 0 } adminEmail)
        {
            await emailSender.SendAsync(
                adminEmail,
                $"New landlord to approve: {owner.FirstName} {owner.LastName}",
                $"{owner.FirstName} {owner.LastName} ({owner.CellPhone}, {owner.Email}) registered as a landlord on Zee Accom." +
                $"{Environment.NewLine}{Environment.NewLine}Their listings stay off the public site until you approve them under Owners.",
                owner.Email,
                cancellationToken);
        }

        return Result<OwnerModel>.Success(owner.Map());
    }
}
