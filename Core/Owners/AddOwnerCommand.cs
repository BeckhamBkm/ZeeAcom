using LazyCache;
using MediatR;
using ZeeAcom.Common.Mappers;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.EntityModels;
using ZeeAcom.Common.Models.OwnerModels;
using ZeeAcom.Core.Entities.Commands;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Owners
{
    public class AddOwnerCommand : IRequest<Result<OwnerModel>>
    {
        public required CreateOwnerModel Data { get; set; }
    }

    public class AddOwnerCommandHandler(DatabaseContext databaseContext, IAppCache cache) : IRequestHandler<AddOwnerCommand, Result<OwnerModel>>
    {
        private const string Error = "Error ading an Entity";
        public async Task<Result<OwnerModel>> Handle(AddOwnerCommand request, CancellationToken cancellationToken)
        {
            var owner = request.Data.Map();

            
            if (owner.Entities != null)
            {
                foreach (var entity in owner.Entities)
                {
                    entity.Owner = null;
                    entity.OwnerId = Guid.Empty;
                    if (entity.Pictures != null)
                    {
                        foreach (var picture in entity.Pictures)
                        {
                            picture.Entity = null;
                        }
                    }
                }
            }

            databaseContext.Owners.Add(owner);
            var ownerSaveResult = await databaseContext.SaveChangesAsync(cancellationToken);
            if (ownerSaveResult <= 0)
            {
                return Result<OwnerModel>.Failure(Error);
            }

            if (owner.Entities != null)
            {
                foreach (var entity in owner.Entities)
                {
                    entity.OwnerId = owner.Id;

                    if (databaseContext.Entry(entity).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
                    {
                        databaseContext.Entities.Add(entity);
                    }

                    if (entity.Pictures != null)
                    {
                        foreach (var picture in entity.Pictures)
                        {
                            picture.EntityId = entity.Id;
                            if (databaseContext.Entry(picture).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
                            {
                                databaseContext.Pictures.Add(picture);
                            }
                        }
                    }
                }
            }

            var entitySaveResult = await databaseContext.SaveChangesAsync(cancellationToken);
            if (entitySaveResult <= 0)
            {
                return Result<OwnerModel>.Failure(Error);
            }

            // _cache.Remove(...);

            return Result<OwnerModel>.Success(owner.Map());
        }
    }
}


