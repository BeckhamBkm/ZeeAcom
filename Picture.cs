using System;
using System.Collections.Generic;

namespace ZeeAcom;

public partial class Picture
{
    public Guid Id { get; set; }

    public string Url { get; set; } = null!;

    public DateTime DateUploaded { get; set; }

    public Guid EntityId { get; set; }
    public Entity Entity { get; set; }
}
