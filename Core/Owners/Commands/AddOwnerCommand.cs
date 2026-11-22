using LazyCache;
using MediatR;
using ZeeAcom.Common.Mappers;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.PropertyModels;
using ZeeAcom.Common.Models.OwnerModels;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Owners.Commands
{
    public class AddOwnerCommand : IRequest<Result<OwnerModel>>
    {
        public required CreateOwnerModel Data { get; set; }
    }

    public class AddOwnerCommandHandler(DatabaseContext databaseContext, IAppCache cache) : IRequestHandler<AddOwnerCommand, Result<OwnerModel>>
    {
        private const string Error = "Error adding an owner";
        public async Task<Result<OwnerModel>> Handle(AddOwnerCommand request, CancellationToken cancellationToken)
        {
            var owner = request.Data.Map();

            // Owners the admin adds don't need approving.
            owner.Status = ZeeAcom.Common.Entities.OwnerStatus.Approved;

            if (owner.Properties != null)
            {
                foreach (var property in owner.Properties)
                {
                    property.Owner = null!;
                    property.OwnerId = Guid.Empty;
                    if (property.Pictures != null)
                    {
                        foreach (var picture in property.Pictures)
                        {
                            picture.Property = null!;
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

            if (owner.Properties != null)
            {
                foreach (var property in owner.Properties)
                {
                    property.OwnerId = owner.Id;

                    if (databaseContext.Entry(property).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
                    {
                        databaseContext.Properties.Add(property);
                    }

                    if (property.Pictures != null)
                    {
                        foreach (var picture in property.Pictures)
                        {
                            picture.PropertyId = property.Id;
                            if (databaseContext.Entry(picture).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
                            {
                                databaseContext.Pictures.Add(picture);
                            }
                        }
                    }
                }
            }

            // Only save again if the property/picture loop actually queued something;
            // an owner with no properties has nothing left to save, which is not an error.
            if (databaseContext.ChangeTracker.HasChanges())
            {
                var propertySaveResult = await databaseContext.SaveChangesAsync(cancellationToken);
                if (propertySaveResult <= 0)
                {
                    return Result<OwnerModel>.Failure(Error);
                }
            }

            // _cache.Remove(...);

            return Result<OwnerModel>.Success(owner.Map());
        }
    }
}


