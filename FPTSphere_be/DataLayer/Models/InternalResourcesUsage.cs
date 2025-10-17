using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class InternalResourcesUsage
{
    public int UsageId { get; set; }

    public int EventId { get; set; }

    public string ResourceName { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int Quantity { get; set; }

    public virtual Event Event { get; set; } = null!;
}
