using LazyCache;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZeeAcom.Common;
using ZeeAcom.Common.Extensions;
using ZeeAcom.Common.Mappers;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.EntityModels;
using ZeeAcom.Common.Models.OwnerModels;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Owners.Queries;

public class GetOwnersQuery : IRequest<Result<IEnumerable<OwnerModel>>>
{
    public required SearchOwnerModel Data { get; set; }
}
   

public class GetOwnersQueryHandler(DatabaseContext databaseContext, IAppCache cache) : IRequestHandler<GetOwnersQuery, Result<IEnumerable<OwnerModel>>>
{   
private readonly TimeSpan CacheExpiry = new(12, 0, 0);

    public async Task<Result<IEnumerable<OwnerModel>>> Handle(GetOwnersQuery request,CancellationToken cancellationToken)
    {
        var entity = request.Data!;

        Task<IEnumerable<Owner>> DataDelegate() => this.GetData();

        //var cachedData = await cache.GetOrAddAsync(CacheData.CacheKey,DataDelegate,this.CacheExpiry);

        //if (cachedData is not null)
        //{
        //    var data = cachedData.AsQueryable()
        //        .FilterByFirstName(entity.FirstName)
        //        .FilterByLastName(entity.LastName)
        //        .ToList();

        //    return Result<IEnumerable<OwnerModel>>.Success(data.Map(),cachedData!.Count());    
        //}

        if (!string.IsNullOrWhiteSpace(entity.OrderBy)) entity.OrderBy = "DateCreated desc";

        var entities = databaseContext.Owners.Select(x => x)
            .AsNoTracking()
            .FilterByFirstName(entity.FirstName)
            .FilterByLastName(entity.LastName);
            //.OrderBy(entity.OrderBy);


        var count = entities.Count();
        var paged = await entities.ApplyPaging(entity.PagingArgs).ToListAsync(cancellationToken);
        return Result<IEnumerable<OwnerModel>>.Success(paged.Map(),count);
    }


    private async Task<IEnumerable<Owner>> GetData()
    {
        var entities = await databaseContext.Owners
            .Select(e=>e)
            .AsNoTracking()
            .ToListAsync();

        return entities;
    }
}