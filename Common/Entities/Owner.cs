
namespace ZeeAcom.Common.Models.OwnerModels;
public sealed class Owner
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string CellPhone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
