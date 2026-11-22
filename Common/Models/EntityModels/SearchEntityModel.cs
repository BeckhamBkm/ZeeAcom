namespace ZeeAcom.Common.Models.EntityModels;

public class SearchEntityModel
{
    public string? Name { get; set; }
    public string? Suburb { get; set; }
    public Guid? OwnerId { get; set; }
    public bool? Active { get; set; }

    public PagingArgs PagingArgs { get; set; } = PagingArgs.NoPaging;
}
