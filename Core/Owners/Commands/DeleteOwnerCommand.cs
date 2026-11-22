using LazyCache;
using MediatR;
using System.Reflection.Metadata;
using ZeeAcom.Common.Models;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Owners.Commands;

public class DeleteOwnerCommand : IRequest<Result>
{
    public required Guid Id { get; set; }
}

public class DeleteOwnerCommandHandler(DatabaseContext databaseContext,IAppCache cache) : IRequestHandler<DeleteOwnerCommand,Result>
{
    private const string Error = "Error deleteing an owner record";
    public async Task<Result> Handle(DeleteOwnerCommand request,CancellationToken cancellationToken)
    {
        var owner = await databaseContext.Owners.FindAsync(request.Id,cancellationToken);

        if(owner is not null)
        {
            databaseContext.Owners.Remove(owner);
            var result = await databaseContext.SaveChangesAsync(cancellationToken);

            //cache.Remove();
            return result > 0 ? Result.Success() : Result.Failure(Error);
        }
        return Result.Failure(Error);
    }
}
