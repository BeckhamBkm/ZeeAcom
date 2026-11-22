using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZeeAcom.Common.Entities;
using ZeeAcom.Common.Models;
using ZeeAcom.Common.Models.EnquiryModels;
using ZeeAcom.Core.Listings.Queries;
using ZeeAcom.Core.Notifications;
using ZeeAcom.DataAccess;

namespace ZeeAcom.Core.Enquiries.Commands;

/// <summary>
/// Saves an enquiry for a publicly visible listing (so the landlord always sees it on their dashboard)
/// and emails it to the landlord. A failed email doesn't fail the enquiry.
/// </summary>
public class SendEnquiryCommand : IRequest<Result>
{
    public required Guid PropertyId { get; set; }

    public required CreateEnquiryModel Data { get; set; }
}

public class SendEnquiryCommandHandler(DatabaseContext databaseContext, IEmailSender emailSender)
    : IRequestHandler<SendEnquiryCommand, Result>
{
    public async Task<Result> Handle(SendEnquiryCommand request, CancellationToken cancellationToken)
    {
        var listing = await databaseContext.Properties
            .PubliclyVisible()
            .Where(p => p.Id == request.PropertyId)
            .Select(p => new { p.Id, p.Name, p.Address, OwnerFirstName = p.Owner.FirstName, OwnerEmail = p.Owner.Email })
            .FirstOrDefaultAsync(cancellationToken);

        if (listing is null)
        {
            return Result.Failure("Listing not found");
        }

        var data = request.Data;
        var enquiry = new Enquiry
        {
            Id = Guid.NewGuid(),
            PropertyId = listing.Id,
            Name = data.Name.Trim(),
            CellPhone = data.CellPhone.Trim(),
            Email = string.IsNullOrWhiteSpace(data.Email) ? null : data.Email.Trim(),
            Message = data.Message.Trim(),
            DateCreated = DateTime.UtcNow,
        };

        databaseContext.Enquiries.Add(enquiry);
        await databaseContext.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(listing.OwnerEmail))
        {
            var body = new StringBuilder()
                .AppendLine($"Hi {listing.OwnerFirstName},")
                .AppendLine()
                .AppendLine($"Someone is interested in {listing.Name} ({listing.Address}):")
                .AppendLine()
                .AppendLine($"Name:  {enquiry.Name}")
                .AppendLine($"Phone: {enquiry.CellPhone}")
                .AppendLine($"Email: {enquiry.Email ?? "-"}")
                .AppendLine()
                .AppendLine(enquiry.Message)
                .AppendLine()
                .AppendLine("You can also see your enquiries under My listings on Zee Accom.")
                .ToString();

            await emailSender.SendAsync(listing.OwnerEmail, $"New enquiry: {listing.Name}", body, enquiry.Email, cancellationToken);
        }

        return Result.Success();
    }
}
