using MediatR;
using ZeeAcom.Common.Models;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Properties.Commands;

public class DeletePropertyCommand : IRequest<Result>
{
    public required Guid Id { get; set; }
}

public class DeletePropertyCommandHandler(DatabaseContext databaseContext) : IRequestHandler<DeletePropertyCommand, Result>
{
    private const string Error = "Error deleting a property record";

    public async Task<Result> Handle(DeletePropertyCommand request, CancellationToken cancellationToken)
    {
        var property = await databaseContext.Properties.FindAsync([request.Id], cancellationToken);
        if (property is null)
        {
            return Result.Failure("Property not found");
        }

        databaseContext.Properties.Remove(property);
        var result = await databaseContext.SaveChangesAsync(cancellationToken);
        return result > 0 ? Result.Success() : Result.Failure(Error);
    }
}
