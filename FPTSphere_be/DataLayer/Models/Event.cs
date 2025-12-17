using System;
using System.Collections.Generic;

namespace DataLayer.Models;

public partial class Event
{
    public int EventId { get; set; }

    public string EventName { get; set; } = null!;

    public string? Description { get; set; }

    public string? BannerUrl { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int? LocationId { get; set; }

    public int? ExternalLocationId { get; set; }

    public int CreatedBy { get; set; }

    public int StatusId { get; set; }

    public int? ParentEventId { get; set; }

    public int? TemplateId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? IsDeleted { get; set; }

    public int? ExpectedAttendees { get; set; }

    public decimal? EstimatedCost { get; set; }

    public virtual ICollection<AttendanceToken> AttendanceTokens { get; set; } = new List<AttendanceToken>();

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<EventAiresult> EventAiresults { get; set; } = new List<EventAiresult>();

    public virtual ICollection<EventApproval> EventApprovals { get; set; } = new List<EventApproval>();

    public virtual ICollection<EventAttendance> EventAttendances { get; set; } = new List<EventAttendance>();

    public virtual ICollection<EventInvitation> EventInvitations { get; set; } = new List<EventInvitation>();

    public virtual ICollection<EventLog> EventLogs { get; set; } = new List<EventLog>();

    public virtual ICollection<EventResource> EventResources { get; set; } = new List<EventResource>();

    public virtual ICollection<EventTask> EventTasks { get; set; } = new List<EventTask>();

    public virtual ExternalLocation? ExternalLocation { get; set; }

    public virtual ICollection<ExternalService> ExternalServices { get; set; } = new List<ExternalService>();

    public virtual ICollection<Event> InverseParentEvent { get; set; } = new List<Event>();

    public virtual Location? Location { get; set; }

    public virtual Event? ParentEvent { get; set; }

    public virtual EventStatus Status { get; set; } = null!;

    public virtual ICollection<StudentFeedbackHeader> StudentFeedbackHeaders { get; set; } = new List<StudentFeedbackHeader>();

    public virtual FeedbackTemplate? Template { get; set; }
}
