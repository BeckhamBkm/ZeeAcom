using System;
using System.Collections.Generic;

namespace ZeeAcom;

public partial class Owner
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string CellPhone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public virtual ICollection<Entity> Entities { get; set; } = new List<Entity>();
}
