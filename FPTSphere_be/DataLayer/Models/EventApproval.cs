using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class EventApproval
{
    public int ApprovalId { get; set; }

    public int EventId { get; set; }

    public int DirectorId { get; set; }

    public string ApprovalStatus { get; set; } = null!;

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User Director { get; set; } = null!;

    public virtual Event Event { get; set; } = null!;
}
