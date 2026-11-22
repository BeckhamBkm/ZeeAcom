using Microsoft.AspNetCore.Identity;

namespace ZeeAcom.Common.Entities;

/// <summary>
/// A login account (admin or landlord). Landlord accounts are linked to an <see cref="Owner"/> via Owner.UserId.
/// </summary>
public class ApplicationUser : IdentityUser
{
}
