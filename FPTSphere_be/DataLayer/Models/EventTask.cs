using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class EventTask
{
    public int TaskId { get; set; }

    public int EventId { get; set; }

    public int? AssignedTo { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? Report { get; set; }

    public int? ParentTaskId { get; set; }

    public bool IsTemplate { get; set; }

    public virtual User? AssignedToNavigation { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual ICollection<EventTask> InverseParentTask { get; set; } = new List<EventTask>();

    public virtual EventTask? ParentTask { get; set; }
}
