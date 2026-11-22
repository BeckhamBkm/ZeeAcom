namespace ZeeAcom.Common.Models.PropertyModels;

public class SearchPropertyModel
{
    public string? Name { get; set; }
    public string? Suburb { get; set; }
    public Guid? OwnerId { get; set; }
    public bool? Active { get; set; }

    public PagingArgs PagingArgs { get; set; } = PagingArgs.NoPaging;
}
