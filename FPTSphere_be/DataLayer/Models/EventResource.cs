using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class EventResource
{
    public int EventResourceId { get; set; }

    public int EventId { get; set; }

    public int ResourceId { get; set; }

    public int QuantityUsed { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual Resource Resource { get; set; } = null!;
}
