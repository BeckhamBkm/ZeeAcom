namespace ZeeAcom.Common.Models.EnquiryModels;

public sealed class EnquiryModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CellPhone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
    public Guid PropertyId { get; set; }
    public string PropertyName { get; set; } = string.Empty;
}
