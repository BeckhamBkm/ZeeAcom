using ZeeAcom.Common.Entities;
using ZeeAcom.Common.Models.OwnerModels;

namespace ZeeAcom.Web.Models;

public sealed class OwnerIndexViewModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public OwnerStatus? Status { get; set; }

    /// <summary>Landlords waiting for approval (shown as a banner whatever the filter).</summary>
    public int PendingCount { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; }
    public int Total { get; set; }
    public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling(Total / (double)PageSize);

    public IEnumerable<OwnerModel> Owners { get; set; } = [];
}
