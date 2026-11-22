using LazyCache;
using MediatR;
using Microsoft.AspNetCore.Identity;
using ZeeAcom.Common.Entities;
using ZeeAcom.Common.Models;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Owners.Commands;

public class DeleteOwnerCommand : IRequest<Result>
{
    public required Guid Id { get; set; }
}

public class DeleteOwnerCommandHandler(DatabaseContext databaseContext, UserManager<ApplicationUser> userManager, IAppCache cache) : IRequestHandler<DeleteOwnerCommand,Result>
{
    private const string Error = "Error deleteing an owner record";
    public async Task<Result> Handle(DeleteOwnerCommand request,CancellationToken cancellationToken)
    {
        var owner = await databaseContext.Owners.FindAsync([request.Id], cancellationToken);

        if(owner is not null)
        {
            await using var transaction = await databaseContext.Database.BeginTransactionAsync(cancellationToken);

            databaseContext.Owners.Remove(owner);
            var result = await databaseContext.SaveChangesAsync(cancellationToken);
            if (result <= 0)
            {
                return Result.Failure(Error);
            }

            // A landlord's login goes with their owner profile.
            if (owner.UserId is not null && await userManager.FindByIdAsync(owner.UserId) is { } user)
            {
                var deleted = await userManager.DeleteAsync(user);
                if (!deleted.Succeeded)
                {
                    return Result.Failure(deleted.Errors.Select(e => e.Description).ToList());
                }
            }

            await transaction.CommitAsync(cancellationToken);

            //cache.Remove();
            return Result.Success();
        }
        return Result.Failure(Error);
    }
}
