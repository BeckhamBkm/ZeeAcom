using ZeeAcom.Common.Models.OwnerModels;

namespace ZeeAcom.Web.Models;

public sealed class OwnerIndexViewModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; }
    public int Total { get; set; }
    public int TotalPages => PageSize <= 0 ? 1 : (int)Math.Ceiling(Total / (double)PageSize);

    public IEnumerable<OwnerModel> Owners { get; set; } = [];
}
