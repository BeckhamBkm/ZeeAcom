namespace ZeeAcom.Core.Properties.Commands;
using LazyCache;
using MediatR;
using ZeeAcom.Common.Mappers;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.PropertyModels;
using ZeeAcom.DataAccess;

public class AddPropertyCommand : IRequest<Result<PropertyModel>>
{
    public required CreatePropertyModel Data { get; set; }
}

public class AddPropertyCommandHandler(DatabaseContext databaseContext, IAppCache cache) : IRequestHandler<AddPropertyCommand, Result<PropertyModel>>
{
    private const string Error = "Error adding a property";
    public async Task<Result<PropertyModel>> Handle(AddPropertyCommand request, CancellationToken cancellationToken)
    {
        var owner = await databaseContext.Owners.FindAsync([request.Data.OwnerId], cancellationToken);
        if (owner is null)
        {
            return Result<PropertyModel>.Failure("Owner not found");
        }

        var property = request.Data.Map();
        property.DateCreated = DateTime.UtcNow;
        // The mapper doesn't set the Owner navigation; property.Map() below needs it to build PropertyModel.Owner.
        property.Owner = owner;
        databaseContext.Properties.Add(property);
        var result = await databaseContext.SaveChangesAsync(cancellationToken);

        //cache.Remove()
        return result > 0 ? Result<PropertyModel>.Success(property.Map()) : Result<PropertyModel>.Failure(Error);
    }

}

