using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class Task
{
    public int TaskId { get; set; }

    public int EventId { get; set; }

    public int AssignedToUserId { get; set; }

    public string TaskDescription { get; set; } = null!;

    public DateTime DueDate { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? CompletionDate { get; set; }

    public virtual User AssignedToUser { get; set; } = null!;

    public virtual Event Event { get; set; } = null!;
}
