using MediatR;
using Microsoft.EntityFrameworkCore;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.EnquiryModels;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Enquiries.Queries;

/// <summary>Enquiries for one owner's properties, newest first.</summary>
public class GetEnquiriesQuery : IRequest<Result<IEnumerable<EnquiryModel>>>
{
    public required Guid OwnerId { get; set; }
}

public class GetEnquiriesQueryHandler(DatabaseContext databaseContext)
    : IRequestHandler<GetEnquiriesQuery, Result<IEnumerable<EnquiryModel>>>
{
    public async Task<Result<IEnumerable<EnquiryModel>>> Handle(GetEnquiriesQuery request, CancellationToken cancellationToken)
    {
        var enquiries = await databaseContext.Enquiries
            .AsNoTracking()
            .Where(e => e.Property.OwnerId == request.OwnerId)
            .OrderByDescending(e => e.DateCreated)
            .Select(e => new EnquiryModel
            {
                Id = e.Id,
                Name = e.Name,
                CellPhone = e.CellPhone,
                Email = e.Email,
                Message = e.Message,
                DateCreated = e.DateCreated,
                PropertyId = e.PropertyId,
                PropertyName = e.Property.Name,
            })
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<EnquiryModel>>.Success(enquiries, enquiries.Count);
    }
}
