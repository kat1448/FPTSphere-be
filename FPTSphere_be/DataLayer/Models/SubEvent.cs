using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class SubEvent
{
    public int SubeventId { get; set; }

    public int? EventId { get; set; }

    public string? SubeventName { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public virtual Event? Event { get; set; }
}
