using MediatR;
using ZeeAcom.Common.Models;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Entities.Commands;

public class DeleteEntityCommand : IRequest<Result>
{
    public required Guid Id { get; set; }
}

public class DeleteEntityCommandHandler(DatabaseContext databaseContext) : IRequestHandler<DeleteEntityCommand, Result>
{
    private const string Error = "Error deleting a property record";

    public async Task<Result> Handle(DeleteEntityCommand request, CancellationToken cancellationToken)
    {
        var entity = await databaseContext.Entities.FindAsync([request.Id], cancellationToken);
        if (entity is null)
        {
            return Result.Failure("Property not found");
        }

        databaseContext.Entities.Remove(entity);
        var result = await databaseContext.SaveChangesAsync(cancellationToken);
        return result > 0 ? Result.Success() : Result.Failure(Error);
    }
}
