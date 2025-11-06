using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class EventLog
{
    public int LogId { get; set; }

    public int EventId { get; set; }

    public int UserId { get; set; }

    public string Action { get; set; } = null!;

    public string? Details { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
