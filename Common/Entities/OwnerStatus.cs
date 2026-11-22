namespace ZeeAcom.Common.Entities;

/// <summary>
/// Where a landlord stands with the admin. Only Approved landlords' listings are public.
/// </summary>
public enum OwnerStatus
{
    /// <summary>Registered, waiting for the admin to approve them.</summary>
    Pending = 0,

    /// <summary>Listings are public.</summary>
    Approved = 1,

    /// <summary>Can still log in and manage listings, but none of them are public.</summary>
    Hidden = 2,

    /// <summary>Can't log in and none of their listings are public.</summary>
    Blocked = 3,
}
