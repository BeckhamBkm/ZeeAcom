using MediatR;
using Microsoft.EntityFrameworkCore;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.ListingModels;
using ZeeAcom.Common.Models.PictureModels;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Listings.Queries;

/// <summary>One publicly visible listing; hidden or unavailable properties are "not found".</summary>
public class GetListingQuery : IRequest<Result<ListingDetailsModel>>
{
    public required Guid Id { get; set; }
}

public class GetListingQueryHandler(DatabaseContext databaseContext) : IRequestHandler<GetListingQuery, Result<ListingDetailsModel>>
{
    public async Task<Result<ListingDetailsModel>> Handle(GetListingQuery request, CancellationToken cancellationToken)
    {
        var listing = await databaseContext.Properties
            .AsNoTracking()
            .PubliclyVisible()
            .Where(p => p.Id == request.Id)
            .Select(p => new ListingDetailsModel
            {
                Id = p.Id,
                Name = p.Name,
                Address = p.Address,
                Suburb = p.Suburb,
                MonthlyRent = p.MonthlyRent,
                DateCreated = p.DateCreated,
                LandlordFirstName = p.Owner.FirstName,
                LandlordCellPhone = p.Owner.CellPhone,
                Pictures = p.Pictures
                    .OrderBy(x => x.SortOrder).ThenBy(x => x.DateUploaded)
                    .Select(x => new PictureModel
                    {
                        Id = x.Id,
                        Url = x.Url,
                        ThumbnailUrl = x.ThumbnailUrl,
                        Caption = x.Caption,
                        SortOrder = x.SortOrder,
                        DateUploaded = x.DateUploaded,
                        PropertyId = x.PropertyId,
                    })
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        return listing is null
            ? Result<ListingDetailsModel>.Failure("Listing not found")
            : Result<ListingDetailsModel>.Success(listing);
    }
}
