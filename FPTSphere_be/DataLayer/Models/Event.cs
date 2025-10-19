using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class Event
{
    public int EventId { get; set; }

    public int? ParentEventId { get; set; }

    public string EventName { get; set; } = null!;

    public string EventType { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int ManagerUserId { get; set; }

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public virtual EventAiresult? EventAiresult { get; set; }

    public virtual ICollection<EventInvitation> EventInvitations { get; set; } = new List<EventInvitation>();

    public virtual ICollection<EventTask> EventTasks { get; set; } = new List<EventTask>();

    public virtual ICollection<ExternalResource> ExternalResources { get; set; } = new List<ExternalResource>();

    public virtual ICollection<InternalResourcesUsage> InternalResourcesUsages { get; set; } = new List<InternalResourcesUsage>();

    public virtual ICollection<Event> InverseParentEvent { get; set; } = new List<Event>();

    public virtual User ManagerUser { get; set; } = null!;

    public virtual Event? ParentEvent { get; set; }

    public virtual ICollection<StudentFeedback> StudentFeedbacks { get; set; } = new List<StudentFeedback>();
}
