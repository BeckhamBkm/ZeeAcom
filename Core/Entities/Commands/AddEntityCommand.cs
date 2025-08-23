namespace ZeeAcom.Core.Entities.Commands;
using LazyCache;
using MediatR;
using ZeeAcom;
using ZeeAcom.Common.Mappers;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.EntityModels;
using ZeeAcom.DataAccess;

public class AddEntityCommand : IRequest<Result<EntityModel>>
{
    public required CreateEntityModel Data { get; set; }
}

public class AddEntityCommandHandler(DatabaseContext databaseContext, IAppCache cache) : IRequestHandler<AddEntityCommand, Result<EntityModel>>
{
    private const string Error = "Error ading an Entity";
    public async Task<Result<EntityModel>> Handle(AddEntityCommand request, CancellationToken cancellationToken)
    {
        var entity = request.Data.Map();
        entity.DateCreated = DateTime.UtcNow;
        databaseContext.Entities.Add(entity);
        var result = await databaseContext.SaveChangesAsync(cancellationToken);

        //cache.Remove()
        return result > 0 ? Result<EntityModel>.Success(entity.Map()) : Result<EntityModel>.Failure(Error);
    }

}

